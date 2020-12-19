using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.ValueObjects;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;

namespace NFe.Core.NotasFiscais.Services
{
    public class EnviarNotaFiscalService : IEnviaNotaFiscalFacade
    {
        private const string DATE_STRING_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICertificadoRepository _certificadoRepository;

        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IConfiguracaoService _configuracaoService;

        private readonly ICertificateManager _certificateManager;
        private readonly INFeConsulta _nfeConsulta;
        private readonly IServiceFactory _serviceFactory;
        private readonly IEmiteNotaFiscalContingenciaFacade _emiteNotaFiscalContingenciaService;
        private readonly RijndaelManagedEncryption _encryptor;

        public EnviarNotaFiscalService(IConfiguracaoRepository configuracaoRepository,
            INotaFiscalRepository notaFiscalRepository, ICertificadoRepository certificadoRepository,
            IConfiguracaoService configuracaoService, IServiceFactory serviceFactory, INFeConsulta nfeConsulta,
            ICertificateManager certificateManager, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService, RijndaelManagedEncryption encryptor)
        {
            _configuracaoRepository = configuracaoRepository;
            _certificadoRepository = certificadoRepository;
            _configuracaoService = configuracaoService;
            _serviceFactory = serviceFactory;
            _nfeConsulta = nfeConsulta;
            _certificateManager = certificateManager;
            _emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            _encryptor = encryptor;
        }

        public ResultadoEnvio EnviarNotaFiscal(NotaFiscal notaFiscal, string cscId, string csc)
        {
            if (notaFiscal.Identificacao.Ambiente == Ambiente.Homologacao)
                notaFiscal.Produtos[0].Descricao = "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

            var certificadoEntity = _certificadoRepository.GetCertificado();
            X509Certificate2 certificado = PickCertificateBasedOnInstallationType(certificadoEntity);

            if (!IsNotaFiscalValida(notaFiscal, cscId, csc, certificado))
            {
                throw new ArgumentException("Nota fiscal inválida.");
            }

            QrCode qrCode = new QrCode();
            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
            TNFe nfe = null;

            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;
            var digVal = "";

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>", string.Empty);
            XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

            try
            {
                var newNodeXml = PreencherQrCode(notaFiscal, cscId, csc, ref qrCode, digVal, node);

                var document = new XmlDocument();
                document.LoadXml(newNodeXml);
                node = document.DocumentElement;

                var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), notaFiscal.Emitente.Endereco.UF);
                NFeAutorizacao4Soap client = CriarClientWS(notaFiscal, certificado, codigoUf);

                var lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(node.OuterXml);
                nfe = lote.NFe[0];

                TProtNFe protocolo = RetornarProtocoloParaLoteSomenteComUmaNotaFiscal(node, client);

                if (IsSuccess(protocolo))
                {
                    notaFiscal = AtribuirValoresApósEnvioComSucesso(notaFiscal, qrCode, protocolo);
                    var resultadoEnvio = new ResultadoEnvio(notaFiscal, protocolo, qrCode, nfe, node);
                    return resultadoEnvio;
                }
                else
                {
                    if (IsInvoiceDuplicated(protocolo))
                    {
                        var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(notaFiscal.Identificacao.Chave.ToString(), notaFiscal.Emitente.Endereco.CodigoUF, certificado, notaFiscal.Identificacao.Modelo);

                        var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                        var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                        notaFiscal = AtribuirValoresApósEnvioComSucesso(notaFiscal, qrCode, protDeserialized);
                        var resultadoEnvio = new ResultadoEnvio(notaFiscal, protDeserialized, qrCode, nfe, node);
                        return resultadoEnvio;
                    }

                    //Nota continua com status pendente nesse caso
                    var mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", protocolo.infProt.xMotivo);
                    throw new ArgumentException(mensagem);
                }
            }
            catch (Exception e)
            {
                log.Error(e);

                try
                {
                    var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(notaFiscal.Identificacao.Chave.ToString(), notaFiscal.Emitente.Endereco.CodigoUF, certificado, notaFiscal.Identificacao.Modelo);

                    if (retornoConsulta.IsEnviada)
                    {
                        var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                        var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                        notaFiscal = AtribuirValoresApósEnvioComSucesso(notaFiscal, qrCode, protDeserialized);
                        var resultadoEnvio = new ResultadoEnvio(notaFiscal, protDeserialized, qrCode, nfe, node);
                        return resultadoEnvio;
                    }

                    throw;
                }
                catch (Exception retornoConsultaException)
                {
                    log.Error(retornoConsultaException);
                    throw;
                }
            }
            finally
            {
                _configuracaoService.SalvarPróximoNúmeroSérie(notaFiscal.Identificacao.Modelo,
                    notaFiscal.Identificacao.Ambiente);
            }
        }

        private void PublishInvoiceSentInContigencyModeEvent(NotaFiscal notaFiscal, string message)
        {
            var theEvent = new NotaFiscalEmitidaEmContingenciaEvent() { justificativa = message, horário = notaFiscal.Identificacao.DataHoraEmissao };
            MessagingCenter.Send(this, nameof(NotaFiscalEmitidaEmContingenciaEvent), theEvent);
        }

        private static string GetExceptionMessage(Exception e)
        {
            return e.InnerException != null ? e.InnerException.Message : e.Message;
        }

        private void ThrowGenericErrorWhenNotDuplicated(NotaFiscal notaFiscal, XmlNode node, TProtNFe protocolo)
        {
            //Nota continua com status pendente nesse caso
            var mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", protocolo.infProt.xMotivo);
            throw new ArgumentException(mensagem);
        }

        private static bool IsInvoiceDuplicated(TProtNFe protocolo)
        {
            return protocolo.infProt.xMotivo.Contains("Duplicidade");
        }

        private static bool IsSuccess(TProtNFe protocolo)
        {
            return protocolo.infProt.cStat.Equals("100");
        }

        private X509Certificate2 PickCertificateBasedOnInstallationType(Cadastro.Certificado.CertificadoEntity certificadoEntity)
        {
            X509Certificate2 certificado;
            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
            {

                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    _encryptor.DecryptRijndael(certificadoEntity.Senha));
            }
            else
            {
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
            }

            return certificado;
        }

        private static TProtNFe RetornarProtocoloParaLoteSomenteComUmaNotaFiscal(XmlNode node, NFeAutorizacao4Soap client)
        {
            var inValue = new nfeAutorizacaoLoteRequest { nfeDadosMsg = node };

            var result = client.nfeAutorizacaoLote(inValue).nfeResultMsg;
            var retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);
            var protocolo = (TProtNFe)retorno.Item;
            return protocolo;
        }

        private NFeAutorizacao4Soap CriarClientWS(NotaFiscal notaFiscal, X509Certificate2 certificado, CodigoUfIbge codigoUf)
        {
            var servico = _serviceFactory.GetService(notaFiscal.Identificacao.Modelo, Servico.AUTORIZACAO, codigoUf, certificado);
            var client = (NFeAutorizacao4Soap)servico.SoapClient;
            return client;
        }

        private static string PreencherQrCode(NotaFiscal notaFiscal, string cscId, string csc, ref QrCode qrCode, string digVal, XmlNode node)
        {
            string newNodeXml;
            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
            {
                qrCode.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario,
                    digVal, notaFiscal.Identificacao.Ambiente,
                    notaFiscal.Identificacao.DataHoraEmissao,
                    notaFiscal.GetTotal().ToString("F", CultureInfo.InvariantCulture), notaFiscal.GetTotalIcms().ToString("F", CultureInfo.InvariantCulture), cscId, csc, notaFiscal.Identificacao.TipoEmissao);

                newNodeXml = node.InnerXml.Replace("<qrCode />", "<qrCode>" + qrCode + "</qrCode>");
            }
            else
            {
                newNodeXml = node.InnerXml;
            }

            return newNodeXml;
        }

        private static NotaFiscal AtribuirValoresApósEnvioComSucesso(NotaFiscal notaFiscal, QrCode qrCode, TProtNFe protocolo)
        {
            var dataAutorizacao = DateTime.ParseExact(protocolo.infProt.dhRecbto, DATE_STRING_FORMAT, CultureInfo.InvariantCulture);
            notaFiscal.QrCodeUrl = qrCode.ToString();
            notaFiscal.Identificacao.Status = new StatusEnvio(Status.ENVIADA);
            notaFiscal.DhAutorizacao = dataAutorizacao.ToString("dd/MM/yyyy HH:mm:ss");
            notaFiscal.DataHoraAutorização = dataAutorizacao;
            notaFiscal.ProtocoloAutorizacao = protocolo.infProt.nProt;
            return notaFiscal;
        }

        public bool IsNotaFiscalValida(NotaFiscal notaFiscal, string cscId, string csc, X509Certificate2 certificado)
        {
            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;
            var digVal = "";
            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>",
                string.Empty);

            XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

            try
            {
                string newNodeXml;
                if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                {
                    QrCode qrCode = new QrCode();
                    qrCode.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario, digVal,
                        notaFiscal.Identificacao.Ambiente,
                        notaFiscal.Identificacao.DataHoraEmissao,
                        notaFiscal.GetTotal().ToString("F", CultureInfo.InvariantCulture),
                        notaFiscal.GetTotalIcms().ToString("F", CultureInfo.InvariantCulture), cscId,
                        csc, notaFiscal.Identificacao.TipoEmissao);

                    newNodeXml = node.InnerXml.Replace("<qrCode />", "<qrCode>" + qrCode + "</qrCode>");
                }
                else
                {
                    newNodeXml = node.InnerXml;
                }

                var document = new XmlDocument();
                document.LoadXml(newNodeXml);
                node = document.DocumentElement;

                ValidadorXml.ValidarXml(node.OuterXml, "enviNFe_v4.00.xsd");

                return true;
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }
        }
    }
}