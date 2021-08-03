using Akka.Actor;
using Akka.Util;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Xml;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private const string NFE_NAMESPACE = "http://www.portalfiscal.inf.br/nfe";
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly IConsultarNotaFiscalService _nfeConsulta;
        private readonly IServiceFactory _serviceFactory;
        private readonly IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService;

        public XmlNFe XmlNFe { get; private set; }

        private IActorRef replyTo;

        public NotaFiscal NotaFiscal { get; private set; }
        public X509Certificate2 Certificado { get; private set; }

        public EnviarNotaActor(IConfiguracaoRepository configuracaoService, IServiceFactory serviceFactory, IConsultarNotaFiscalService nfeConsulta, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService)
        {
            _configuracaoService = configuracaoService;
            _serviceFactory = serviceFactory;
            _nfeConsulta = nfeConsulta;

            ReceiveAsync<EnviarNotaFiscal>(HandleEnviarNotaFiscal);
            Receive<Result<TProtNFe>>(msg => msg.IsSuccess, HandleSuccess_nfeAutorizacaoLoteResult);
            Receive<Result<TProtNFe>>(msg => !msg.IsSuccess, HandleErro_nfeAutorizacaoLoteResult);
            ReceiveAsync<ReceiveTimeout>(HandleReceiveTimeoutAsync);
            this.emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
        }

        private async Task HandleEnviarNotaFiscal(EnviarNotaFiscal msg)
        {
            replyTo = Sender;
            NotaFiscal = msg.NotaFiscal;
            Certificado = msg.Certificado;
            XmlNFe = msg.XmlNFe;

            if (!IsNotaFiscalValida(msg.NotaFiscal, msg.CscId, msg.Csc, msg.Certificado))
            {
                const string message = "Nota fiscal inválida.";
                log.Error(message);
                replyTo.Tell(new Status.Failure(new ArgumentException(message)));
                return;
            }

            var config = await _configuracaoService.GetConfiguracaoAsync();

            if (config.IsContingencia)
            {
                log.Info("Enviando nota fiscal em modo contingência.");
                NotaFiscal = emiteNotaFiscalContingenciaService.SaveNotaFiscalContingencia(msg.Certificado, config, NotaFiscal, config.CscId, config.Csc, NFE_NAMESPACE);
                replyTo.Tell(new Status.Success(new ResultadoEnvio(NotaFiscal, null, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }
            else
            {
                var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), msg.NotaFiscal.Emitente.Endereco.UF);

                var sefazActor = Context.ActorOf(Props.Create(() => new SefazActor(_serviceFactory)));
                sefazActor.Tell(new SefazActor.nfeAutorizacaoLote(msg.XmlNFe.XmlNode, msg.NotaFiscal, msg.Certificado, codigoUf));
                SetReceiveTimeout(TimeSpan.FromSeconds(30));
            }
        }

        private void PreencheDadosNotaEnviadaAposErroConexao(MensagemRetornoConsulta retorno)
        {
            var protSerialized = XmlUtil.Serialize(retorno.Protocolo.Xml, NFE_NAMESPACE);
            var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

            var notaFiscal = AtribuirValoresApósEnvioComSucesso(NotaFiscal, XmlNFe.QrCode, protDeserialized);
            replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protDeserialized, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
        }

        private void HandleErro_nfeAutorizacaoLoteResult(Result<TProtNFe> obj)
        {
            log.Info("Resultado do envio da nota fiscal recebido.");
            SetReceiveTimeout(null);
            
            log.Error(obj.Exception);

            var retorno = VerificaSeNotaFoiEnviada();

            if (retorno.IsEnviada)
            {
                PreencheDadosNotaEnviadaAposErroConexao(retorno);
            }
            else
            {
                replyTo.Tell(new Status.Failure(obj.Exception));
            }
        }

        private void HandleSuccess_nfeAutorizacaoLoteResult(Result<TProtNFe> obj)
        {
            log.Info("Resultado do envio da nota fiscal recebido.");
            SetReceiveTimeout(null);

            if (IsSuccess(obj.Value))
            {
                var notaFiscal = AtribuirValoresApósEnvioComSucesso(NotaFiscal, XmlNFe.QrCode, obj.Value);
                replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, obj.Value, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }
            else
            {
                if (IsInvoiceDuplicated(obj.Value))
                {
                    var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(NotaFiscal.Identificacao.Chave.ToString(), NotaFiscal.Emitente.Endereco.CodigoUF, Certificado, NotaFiscal.Identificacao.Modelo);

                    var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo.Xml, NFE_NAMESPACE);
                    var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                    var notaFiscal = AtribuirValoresApósEnvioComSucesso(NotaFiscal, XmlNFe.QrCode, protDeserialized);
                    replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protDeserialized, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
                }

                //Nota continua com status pendente nesse caso
                var mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", obj.Value.infProt.xMotivo);
                replyTo.Tell(new Status.Failure(new ArgumentException(mensagem)));
            }
        }

        private async Task HandleReceiveTimeoutAsync(ReceiveTimeout obj)
        {
            SetReceiveTimeout(null);

            var retorno = VerificaSeNotaFoiEnviada();

            if (retorno.IsEnviada)
            {
                PreencheDadosNotaEnviadaAposErroConexao(retorno);
            }
            else
            {
                // contingência

                log.Error("Erro de conexão ao enviar nota fiscal.");

                // Stop execution if model 55
                if (NotaFiscal.Identificacao.Modelo == Modelo.Modelo55)
                {
                    replyTo.Tell(new Status.Failure(new Exception("Timeout ao enviar nota fiscal.")));
                    return;
                }

                var modoOnlineActor = Context.System.ActorSelection("/user/modoOnline");
                modoOnlineActor.Tell(new ModoOnlineActor.AtivarModoOffline("Timeout ao enviar nota fiscal.", NotaFiscal.Identificacao.DataHoraEmissao));

                var config = await _configuracaoService.GetConfiguracaoAsync();

                NotaFiscal = emiteNotaFiscalContingenciaService.SaveNotaFiscalContingencia(Certificado, config, NotaFiscal, config.CscId, config.Csc, NFE_NAMESPACE);

                replyTo.Tell(new Status.Success(new ResultadoEnvio(NotaFiscal, null, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }
        }

        private MensagemRetornoConsulta VerificaSeNotaFoiEnviada()
        {
            MensagemRetornoConsulta retorno = new MensagemRetornoConsulta() { IsEnviada = false };

            try
            {
                log.Info("Tentando verificar se a nota foi enviada após um erro de conexão.");
                retorno = _nfeConsulta.ConsultarNotaFiscal(NotaFiscal.Identificacao.Chave.ToString(), NotaFiscal.Emitente.Endereco.CodigoUF, Certificado, NotaFiscal.Identificacao.Modelo);
            }
            catch (Exception e)
            {
                log.Error("Erro ao tentar verficar se nota foi enviada após um erro de conexão.", e);
            }

            return retorno;
        }

        private static bool IsInvoiceDuplicated(TProtNFe protocolo)
        {
            return protocolo.infProt.xMotivo.Contains("Duplicidade");
        }

        private static bool IsSuccess(TProtNFe protocolo)
        {
            return protocolo.infProt.cStat.Equals("100");
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
