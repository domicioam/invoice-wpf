using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Utils.Xml;
using NFe.Core.Utils.Zip;
using System.Configuration;
using NFe.Core.Utils.PDF;
using NFe.Core;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net.Mail;
using MailKit.Security;
using MediatR;

namespace NFe.WPF.Utils
{
    public class MailManager
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IHistoricoEnvioContabilidadeRepository _historicoEnvioContabilidadeRepository;
        private readonly IEmitenteRepository _emitenteRepository;
        private readonly GeradorZip _geradorZip;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly IMediator mediator;

        public MailManager(IHistoricoEnvioContabilidadeRepository historicoEnvioContabilidadeRepository,
            IEmitenteRepository emitenteRepository, GeradorZip geradorZip, INotaFiscalRepository notaFiscalRepository,
            IMediator mediator)
        {
            _historicoEnvioContabilidadeRepository = historicoEnvioContabilidadeRepository;
            _emitenteRepository = emitenteRepository;
            _geradorZip = geradorZip;
            _notaFiscalRepository = notaFiscalRepository;
            this.mediator = mediator;
        }

        public Task EnviarNotasParaContabilidade(int diaParaEnvio)
        {
            return Task.Run(async () =>
            {
                var historicoPeriodoCount =
                    await _historicoEnvioContabilidadeRepository.GetHistoricoByPeriodoAsync(DateTime.Now.AddMonths(-1));

                if (DateTime.Now.Day >= diaParaEnvio && historicoPeriodoCount == 0)
                {
                    var periodo = DateTime.Now.AddMonths(-1);
                    var periodoStr = periodo.ToString("MM/yyyy");
                    var path = await _geradorZip.GerarZipEnvioContabilidadeAsync(periodo);

                    if (string.IsNullOrWhiteSpace(path))
                        throw new Exception("Ocorreu um erro ao gerar os arquivos das notas fiscais!");

                    try
                    {
                        var emissor = _emitenteRepository.GetEmitente();

                        Email mail = Email.CreateDefaultContabilidadeEmail(periodoStr, emissor.RazaoSocial, emissor.CNPJ);

                        var mailMessage = new MimeMessage();
                        mailMessage.From.Add(new MailboxAddress(mail.From.Name, mail.From.Account));
                        mailMessage.To.Add(new MailboxAddress(mail.To.Name, mail.To.Account));
                        mailMessage.Subject = mail.Subject;

                        var builder = new BodyBuilder();
                        builder.TextBody = mail.Body;
                        builder.Attachments.Add(path);

                        mailMessage.Body = builder.ToMessageBody();

                        using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
                        {
                            smtpClient.Connect("smtp.outlook.com", 587, SecureSocketOptions.StartTls);
                            smtpClient.Authenticate(mail.From.Account, mail.Password);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }

                        _historicoEnvioContabilidadeRepository
                            .Salvar(new HistoricoEnvioContabilidade { DataEnvio = DateTime.Now, Periodo = periodoStr });
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        throw;
                    }
                }
            });
        }

        public async void EnviarEmailDestinatario(string email, string xmlPath, NotaFiscalEntity notaFiscal)
        {
            var xml = await notaFiscal.LoadXmlAsync();
            var notaFiscalCore = _notaFiscalRepository.GetNotaFiscalFromNfeProcXml(xml);

            var command = new GerarDanfeNfceEmailCommand(notaFiscalCore, mediator);
            command.ExecuteAsync();

            if (command.IsExecuted)
            {
                string pdfPath = command.Result;
                await EnviarEmailDestinatario(email, xmlPath, pdfPath);
            }
        }


        private Task EnviarEmailDestinatario(string emailDestinatario, string xmlPath, string pdfPath)
        {
            return Task.Run(() =>
            {
                try
                {
                    var emissor = _emitenteRepository.GetEmitente();
                    var fromAddress = new MailAddress(ConfigurationManager.AppSettings["fromMailAccount"], ConfigurationManager.AppSettings["fromMailName"]);
                    var toAddress = new MailAddress(emailDestinatario, string.Empty);
                    string fromPassword = ConfigurationManager.AppSettings["fromMailPassword"];


                    var mailMessage = new MimeMessage();
                    mailMessage.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["fromMailName"], ConfigurationManager.AppSettings["fromMailAccount"]));
                    mailMessage.To.Add(new MailboxAddress(string.Empty, emailDestinatario));
                    mailMessage.Subject = $"Nota Fiscal Eletrônica - {emissor.RazaoSocial} - {emissor.CNPJ}";

                    var builder = new BodyBuilder();
                    builder.TextBody = "Conforme as regras de Distribuição da NFe descritas no Manual do Contribuinte, segue cópia do XML autorizado pela SEFAZ.\n\nEsta mensagem foi gerada automaticamente.";
                    builder.Attachments.Add(xmlPath);
                    builder.Attachments.Add(pdfPath);

                    mailMessage.Body = builder.ToMessageBody();

                    using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
                    {
                        smtpClient.Connect("smtp.outlook.com", 587, SecureSocketOptions.StartTls);
                        smtpClient.Authenticate(ConfigurationManager.AppSettings["fromMailAccount"], ConfigurationManager.AppSettings["fromMailPassword"]);
                        smtpClient.Send(mailMessage);
                        smtpClient.Disconnect(true);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e);
                    throw;
                }
            });
        }
    }
}