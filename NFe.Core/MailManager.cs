using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Utils.Xml;
using NFe.Core.Utils.Zip;
using System.Configuration;
using NFe.Core.Utils.PDF;
using NFe.Core;

namespace NFe.WPF.Utils
{
    public class MailManager
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IHistoricoEnvioContabilidadeRepository _historicoEnvioContabilidadeRepository;
        private readonly IEmitenteRepository _emitenteRepository;
        private readonly GeradorZip _geradorZip;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        public MailManager(IHistoricoEnvioContabilidadeRepository historicoEnvioContabilidadeRepository,
            IEmitenteRepository emitenteRepository, GeradorZip geradorZip, INotaFiscalRepository notaFiscalRepository)
        {
            _historicoEnvioContabilidadeRepository = historicoEnvioContabilidadeRepository;
            _emitenteRepository = emitenteRepository;
            _geradorZip = geradorZip;
            _notaFiscalRepository = notaFiscalRepository;
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

                        var smtp = new SmtpClient
                        {
                            Host = "smtp.outlook.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(mail.From.Address.Address, mail.Password)
                        };

                        var data = new Attachment(path, MediaTypeNames.Application.Octet);

                        using (var message = new MailMessage(mail.From.Address, mail.To.Address)
                        {
                            Subject = mail.Subject,
                            Body = mail.Body
                        })
                        {
                            message.Attachments.Add(data);
                            smtp.Send(message);
                        }

                        _historicoEnvioContabilidadeRepository
                            .Salvar(new HistoricoEnvioContabilidade { DataEnvio = DateTime.Now, Periodo = periodoStr });
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        if (e.GetType() == typeof(SmtpException))
                            throw new Exception(
                                "Não foi possível enviar o e-mail para a contabilidade, verifique sua conexão com a internet.",
                                e);
                    }
                }
            });
        }

        public async void EnviarEmailDestinatario(string email, string xmlPath, NotaFiscalEntity notaFiscal)
        {
            var xml = await notaFiscal.LoadXmlAsync();
            var notaFiscalCore = _notaFiscalRepository.GetNotaFiscalFromNfeProcXml(xml);

            var pdfPath = GeradorPDF.ObterPdfEnvioNotaFiscalEmail(notaFiscalCore);
            await EnviarEmailDestinatario(email, xmlPath, pdfPath);
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

                    var subject = string.Concat("Nota Fiscal Eletrônica", " - ", emissor.RazaoSocial, " - ",
                        emissor.CNPJ);
                    const string body =
                        "Conforme as regras de Distribuição da NFe descritas no Manual do Contribuinte, segue cópia do XML autorizado pela SEFAZ.\n\nEsta mensagem foi gerada automaticamente.";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.outlook.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };

                    var xml = new Attachment(xmlPath, MediaTypeNames.Application.Octet);
                    var pdf = new Attachment(pdfPath, MediaTypeNames.Application.Pdf);

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        message.Attachments.Add(xml);
                        message.Attachments.Add(pdf);
                        smtp.Send(message);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e);
                    if (e.GetType() == typeof(SmtpException))
                        throw new Exception("Não foi possível enviar o e-mail, verifique sua conexão com a internet.",
                            e);

                    throw;
                }
            });
        }
    }
}