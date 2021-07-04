using Akka.Actor;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;


namespace DgSystems.NFe.Services.Actors
{
    public class EnviarNotaActor : ReceiveActor
    {
        #region Messages
        public class EnviarNotaFiscal
        {
            public EnviarNotaFiscal(NotaFiscal notaFiscal, string cscId, string csc, X509Certificate2 certificado, XmlNFe xmlNFe)
            {
                NotaFiscal = notaFiscal;
                CscId = cscId;
                Csc = csc;
                Certificado = certificado;
                XmlNFe = xmlNFe;
            }

            public NotaFiscal NotaFiscal { get; }
            public string CscId { get; }
            public string Csc { get; }
            public X509Certificate2 Certificado { get; }
            public XmlNFe XmlNFe { get; }
        }
        #endregion

        private const string DATE_STRING_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly IConsultarNotaFiscalService _nfeConsulta;
        private readonly IServiceFactory _serviceFactory;

        public EnviarNotaActor(IConfiguracaoRepository configuracaoService, IServiceFactory serviceFactory, IConsultarNotaFiscalService nfeConsulta)
        {
            _configuracaoService = configuracaoService;
            _serviceFactory = serviceFactory;
            _nfeConsulta = nfeConsulta;

            Receive<EnviarNotaFiscal>(HandleEnviarNotaFiscal);
        }

        private void HandleEnviarNotaFiscal(EnviarNotaFiscal msg)
        {
            if (!IsNotaFiscalValida(msg.NotaFiscal, msg.CscId, msg.Csc, msg.Certificado))
            {
                const string message = "Nota fiscal inválida.";
                log.Error(message);
                Sender.Tell(new Status.Failure(new ArgumentException(message)));
            }

            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            try
            {
                var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), msg.NotaFiscal.Emitente.Endereco.UF);
                NFeAutorizacao4Soap client = CriarClientWS(msg.NotaFiscal, msg.Certificado, codigoUf);
                TProtNFe protocolo = InvocaServico_E_RetornaProtocolo(msg.XmlNFe.XmlNode, client);

                if (IsSuccess(protocolo))
                {
                    var notaFiscal = AtribuirValoresApósEnvioComSucesso(msg.NotaFiscal, msg.XmlNFe.QrCode, protocolo);
                    Sender.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protocolo, msg.XmlNFe.QrCode, msg.XmlNFe.TNFe, msg.XmlNFe.XmlNode)));
                }
                else
                {
                    if (IsInvoiceDuplicated(protocolo))
                    {
                        var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(msg.NotaFiscal.Identificacao.Chave.ToString(), msg.NotaFiscal.Emitente.Endereco.CodigoUF, msg.Certificado, msg.NotaFiscal.Identificacao.Modelo);

                        var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                        var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                        var notaFiscal = AtribuirValoresApósEnvioComSucesso(msg.NotaFiscal, msg.XmlNFe.QrCode, protDeserialized);
                        Sender.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protDeserialized, msg.XmlNFe.QrCode, msg.XmlNFe.TNFe, msg.XmlNFe.XmlNode)));
                    }

                    //Nota continua com status pendente nesse caso
                    var mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", protocolo.infProt.xMotivo);
                    Sender.Tell(new Status.Failure(new ArgumentException(mensagem)));
                }
            }
            catch (Exception e)
            {
                log.Error(e);

                try
                {
                    var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(msg.NotaFiscal.Identificacao.Chave.ToString(), msg.NotaFiscal.Emitente.Endereco.CodigoUF, msg.Certificado, msg.NotaFiscal.Identificacao.Modelo);

                    if (retornoConsulta.IsEnviada)
                    {
                        var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                        var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                        var notaFiscal = AtribuirValoresApósEnvioComSucesso(msg.NotaFiscal, msg.XmlNFe.QrCode, protDeserialized);
                        Sender.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protDeserialized, msg.XmlNFe.QrCode, msg.XmlNFe.TNFe, msg.XmlNFe.XmlNode)));
                    }
                    else
                    {
                        Sender.Tell(new Status.Failure(e));
                    }
                }
                catch (Exception retornoConsultaException)
                {
                    log.Error(retornoConsultaException);
                    Sender.Tell(new Status.Failure(retornoConsultaException));
                }
            }
            finally
            {
                _configuracaoService.SalvarPróximoNúmeroSérie(msg.NotaFiscal.Identificacao.Modelo,
                    msg.NotaFiscal.Identificacao.Ambiente);
            }
        }

        private static bool IsInvoiceDuplicated(TProtNFe protocolo)
        {
            return protocolo.infProt.xMotivo.Contains("Duplicidade");
        }

        private static bool IsSuccess(TProtNFe protocolo)
        {
            return protocolo.infProt.cStat.Equals("100");
        }

        private static TProtNFe InvocaServico_E_RetornaProtocolo(XmlNode node, NFeAutorizacao4Soap client)
        {
            var inValue = new nfeAutorizacaoLoteRequest { nfeDadosMsg = node };

            var result = client.nfeAutorizacaoLote(inValue).nfeResultMsg;
            var retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);
            return (TProtNFe)retorno.Item;
        }

        private NFeAutorizacao4Soap CriarClientWS(NotaFiscal notaFiscal, X509Certificate2 certificado, CodigoUfIbge codigoUf)
        {
            var servico = _serviceFactory.GetService(notaFiscal.Identificacao.Modelo, Servico.AUTORIZACAO, codigoUf, certificado);
            return (NFeAutorizacao4Soap)servico.SoapClient;
        }

        private static NotaFiscal AtribuirValoresApósEnvioComSucesso(NotaFiscal notaFiscal, QrCode qrCode, TProtNFe protocolo)
        {
            var dataAutorizacao = DateTime.ParseExact(protocolo.infProt.dhRecbto, DATE_STRING_FORMAT, CultureInfo.InvariantCulture);
            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                notaFiscal.QrCodeUrl = qrCode.ToString();
            notaFiscal.Identificacao.Status = new StatusEnvio(global::NFe.Core.Entitities.Status.ENVIADA);
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
