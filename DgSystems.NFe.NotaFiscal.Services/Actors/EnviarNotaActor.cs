using Akka.Actor;
using Akka.Util;
using AutoMapper;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
        private readonly IConfiguracaoRepository configuracaoService;
        private readonly IConsultarNotaFiscalService nfeConsulta;
        private readonly IServiceFactory serviceFactory;
        private readonly IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService;
        private readonly IMapper mapper;
        private readonly Func<IUntypedActorContext, IActorRef> contingenciaMaker;
        private readonly IActorRef modoOnlineActor;

        public XmlNFe XmlNFe { get; private set; }

        private IActorRef replyTo;

        public NotaFiscal NotaFiscal { get; private set; }
        public X509Certificate2 Certificado { get; private set; }

        public EnviarNotaActor(IConfiguracaoRepository configuracaoService, IServiceFactory serviceFactory,
            IConsultarNotaFiscalService nfeConsulta, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService,
            IMapper mapper, Func<IUntypedActorContext, IActorRef> contingenciaMaker, IActorRef modoOnlineActor)
        {
            this.configuracaoService = configuracaoService;
            this.serviceFactory = serviceFactory;
            this.nfeConsulta = nfeConsulta;

            ReceiveAsync<EnviarNotaFiscal>(HandleEnviarNotaFiscal);
            Receive<Result<TProtNFe>>(msg => msg.IsSuccess, HandleSuccess_nfeAutorizacaoLoteResult);
            Receive<Result<TProtNFe>>(HandleErro_nfeAutorizacaoLoteResult);
            ReceiveAsync<ReceiveTimeout>(HandleReceiveTimeoutAsync);
            this.emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            this.mapper = mapper;
            this.contingenciaMaker = contingenciaMaker;
            this.modoOnlineActor = modoOnlineActor;
        }

        private async Task HandleEnviarNotaFiscal(EnviarNotaFiscal msg)
        {
            replyTo = Sender;
            NotaFiscal = msg.NotaFiscal;
            Certificado = msg.Certificado;
            XmlNFe = msg.XmlNFe;

            if (!msg.NotaFiscal.IsNotaFiscalValida(msg.CscId, msg.Csc, msg.Certificado, NFE_NAMESPACE))
            {
                const string message = "Nota fiscal inválida.";
                log.Error(message);
                replyTo.Tell(new Status.Failure(new ArgumentException(message)));
                return;
            }

            var config = await configuracaoService.GetConfiguracaoAsync();

            if (config.IsContingencia)
            {
                log.Info("Enviando nota fiscal em modo contingência.");
                NotaFiscal = await emiteNotaFiscalContingenciaService.SaveNotaFiscalContingenciaAsync(msg.Certificado, config, NotaFiscal, config.CscId, config.Csc, NFE_NAMESPACE);
                replyTo.Tell(new Status.Success(new ResultadoEnvio(NotaFiscal, null, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }
            else
            {
                var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), msg.NotaFiscal.Emitente.Endereco.UF);

                var sefazActor = Context.ActorOf(Props.Create(() => new SefazActor(serviceFactory)));
                sefazActor.Tell(new SefazActor.nfeAutorizacaoLote(msg.XmlNFe.XmlNode, msg.NotaFiscal, msg.Certificado, codigoUf));
                SetReceiveTimeout(TimeSpan.FromSeconds(20));
            }
        }

        private (NotaFiscal notafiscal, TProtNFe protDeserialized) PreencheDadosNotaEnviadaAposErroConexao(RetornoConsulta retorno)
        {
            var protDeserialized = mapper.Map<TProtNFe>(retorno.Protocolo.protNFe);
            NotaFiscal notaFiscal = AtribuirValoresApósEnvioComSucesso(NotaFiscal, XmlNFe.QrCode, protDeserialized);
            return (notaFiscal, protDeserialized);
        }

        private void HandleErro_nfeAutorizacaoLoteResult(Result<TProtNFe> obj)
        {
            log.Info("Resultado do envio da nota fiscal recebido.");
            SetReceiveTimeout(null);

            log.Error(obj.Exception);

            RetornoConsulta retorno = VerificaSeNotaFoiEnviada();
            if (retorno.IsEnviada)
            {
                (NotaFiscal notaFiscal, TProtNFe protDeserialized) = PreencheDadosNotaEnviadaAposErroConexao(retorno);
                replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protDeserialized, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }
            else
            {
                replyTo.Tell(new Status.Failure(obj.Exception));
            }
        }

        private void HandleSuccess_nfeAutorizacaoLoteResult(Result<TProtNFe> msg)
        {
            log.Info("Resultado do envio da nota fiscal recebido.");
            SetReceiveTimeout(null);

            if (IsSuccess(msg.Value))
            {
                var notaFiscal = AtribuirValoresApósEnvioComSucesso(NotaFiscal, XmlNFe.QrCode, msg.Value);
                replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, msg.Value, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
                return;
            }

            if (IsInvoiceDuplicated(msg.Value))
            {
                var retornoConsulta = nfeConsulta.ConsultarNotaFiscal(NotaFiscal.Identificacao.Chave.ToString(), NotaFiscal.Emitente.Endereco.CodigoUF, Certificado, NotaFiscal.Identificacao.Modelo);
                var notaFiscal = AtribuirValoresApósEnvioComSucesso(NotaFiscal, XmlNFe.QrCode, mapper.Map<TProtNFe>(retornoConsulta.Protocolo.protNFe));
                replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, mapper.Map<TProtNFe>(retornoConsulta.Protocolo.protNFe), XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }

            var mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", msg.Value.infProt.xMotivo);
            replyTo.Tell(new Status.Failure(new ArgumentException(mensagem)));
        }

        private async Task HandleReceiveTimeoutAsync(ReceiveTimeout obj)
        {
            SetReceiveTimeout(null);
            
            RetornoConsulta retorno = VerificaSeNotaFoiEnviada();
            if (retorno.IsEnviada)
            {
                (NotaFiscal notaFiscal, TProtNFe protDeserialized) = PreencheDadosNotaEnviadaAposErroConexao(retorno);
                replyTo.Tell(new Status.Success(new ResultadoEnvio(notaFiscal, protDeserialized, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
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

                var config = await configuracaoService.GetConfiguracaoAsync();
                NotaFiscal = await emiteNotaFiscalContingenciaService.SaveNotaFiscalContingenciaAsync(Certificado, config, NotaFiscal, config.CscId, config.Csc, NFE_NAMESPACE);

                replyTo.Tell(new Status.Success(new ResultadoEnvio(NotaFiscal, null, XmlNFe.QrCode, XmlNFe.TNFe, XmlNFe.XmlNode)));
            }
        }

        private RetornoConsulta VerificaSeNotaFoiEnviada()
        {
            RetornoConsulta retorno = new RetornoConsulta() { IsEnviada = false };

            try
            {
                log.Info("Tentando verificar se a nota foi enviada após um erro de conexão.");
                retorno = nfeConsulta.ConsultarNotaFiscal(NotaFiscal.Identificacao.Chave.ToString(), NotaFiscal.Emitente.Endereco.CodigoUF, Certificado, NotaFiscal.Identificacao.Modelo);
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
    }
}
