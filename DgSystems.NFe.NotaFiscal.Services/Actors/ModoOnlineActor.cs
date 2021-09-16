using Akka.Actor;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using System;
using System.Collections.Generic;

namespace DgSystems.NFe.Services.Actors
{
    public class ModoOnlineActor : ReceiveActor
    {
        #region Messages
        private class Tick { }
        public class Start { }
        public class AtivarModoOnline { }
        public class AtivarModoOffline
        {
            public AtivarModoOffline(string justificativa, DateTime dataHoraContingencia)
            {
                DataHoraContingencia = dataHoraContingencia;
                Justificativa = justificativa;
            }

            public DateTime DataHoraContingencia { get; }
            public string Justificativa { get; }
        }
        #endregion

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IConfiguracaoRepository configuracaoRepository;
        private readonly IConsultaStatusServicoSefazService consultaStatusServicoService;
        private readonly IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService;
        private readonly INotaFiscalRepository notaFiscalRepository;
        private readonly IEmitenteRepository emissorService;
        private readonly IConsultarNotaFiscalService nfeConsulta;
        private readonly IServiceFactory serviceFactory;
        private readonly CertificadoService certificadoService;
        private readonly SefazSettings sefazSettings;
        private readonly Func<IUntypedActorContext, IActorRef> contingenciaMaker;
        private IActorRef emiteNfeContingenciaActor;

        public ModoOnlineActor(IConfiguracaoRepository configuracaoRepository, IConsultaStatusServicoSefazService consultaStatusServicoService,
            INotaFiscalRepository notaFiscalRepository, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService, IEmitenteRepository emissorService,
            IConsultarNotaFiscalService nfeConsulta, IServiceFactory serviceFactory, CertificadoService certificadoService, SefazSettings sefazSettings,
            Func<IUntypedActorContext, IActorRef> contingenciaMaker)
        {
            this.notaFiscalRepository = notaFiscalRepository;
            this.configuracaoRepository = configuracaoRepository;
            this.consultaStatusServicoService = consultaStatusServicoService;
            this.emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            this.emissorService = emissorService;
            this.nfeConsulta = nfeConsulta;
            this.serviceFactory = serviceFactory;
            this.certificadoService = certificadoService;
            this.sefazSettings = sefazSettings;
            this.contingenciaMaker = contingenciaMaker;

            Receive<Start>(HandleStart);
            Receive<Tick>(HandleTick);
            Receive<AtivarModoOffline>(HandleAtivarModoOffline);
            Receive<AtivarModoOnline>(HandleAtivarModoOnline);
            Receive<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(HandleResultadoNotasTransmitidas);
            Receive<ReceiveTimeout>(HandleReceiveTimeout);
        }

        private void HandleReceiveTimeout(ReceiveTimeout msg)
        {
            SetReceiveTimeout(null);
            var theEvent = new NotasFiscaisTransmitidasEvent() { MensagensErro = new List<string> { "Erro ao tentar transmitir notas emitidas em contingência." } };
            MessagingCenter.Send(this, nameof(NotasFiscaisTransmitidasEvent), theEvent);
            log.Error("Erro ao tentar transmitir as notas emitidas em contingência.");

            Context.Stop(emiteNfeContingenciaActor);
        }

        private void HandleResultadoNotasTransmitidas(EmiteNFeContingenciaActor.ResultadoNotasTransmitidas msg)
        {
            try
            {
                if (msg.Erros == null) return;
                
                var configuração = configuracaoRepository.GetConfiguracao();
                NotaFiscalEntity primeiraNotaContingencia = notaFiscalRepository.GetPrimeiraNotaEmitidaEmContingencia(configuração.DataHoraEntradaContingencia, DateTime.Now);
                NotaFiscalEntity notaParaCancelar = null;

                if (primeiraNotaContingencia != null)
                {
                    int numero = int.Parse(primeiraNotaContingencia.Numero) - 1;
                    notaParaCancelar = notaFiscalRepository.GetNota(numero.ToString(), primeiraNotaContingencia.Serie,
                        primeiraNotaContingencia.Modelo);
                }

                emiteNotaFiscalContingenciaService.InutilizarCancelarNotasPendentesContingencia(notaParaCancelar, notaFiscalRepository);

                var theEvent = new NotasFiscaisTransmitidasEvent() { MensagensErro = msg.Erros };
                MessagingCenter.Send(this, nameof(NotasFiscaisTransmitidasEvent), theEvent);
            }
            catch (Exception e)
            {
                var theEvent = new NotasFiscaisTransmitidasEvent() { MensagensErro = new List<string> { "Erro ao tentar transmitir notas emitidas em contingência." } };
                MessagingCenter.Send(this, nameof(NotasFiscaisTransmitidasEvent), theEvent);
                log.Error("Erro ao tentar transmitir as notas emitidas em contingência.", e);
            }
            finally
            {
                Context.Stop(emiteNfeContingenciaActor);
            }
        }

        private void HandleAtivarModoOnline(AtivarModoOnline msg)
        {
            var configuração = configuracaoRepository.GetConfiguracao();

            configuração.IsContingencia = false;
            configuracaoRepository.Salvar(configuração);

            SetReceiveTimeout(TimeSpan.FromSeconds(30));
            emiteNfeContingenciaActor = contingenciaMaker(Context);
            emiteNfeContingenciaActor.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());
        }

        private void HandleAtivarModoOffline(AtivarModoOffline msg)
        {
            var config = configuracaoRepository.GetConfiguracao();

            if (!config.IsContingencia)
            {
                config.IsContingencia = true;
                config.DataHoraEntradaContingencia = msg.DataHoraContingencia;
                config.JustificativaContingencia = msg.Justificativa;
                configuracaoRepository.Salvar(config);
            }

            var theEvent = new ServicoOfflineEvent();
            MessagingCenter.Send(this, nameof(ServicoOfflineEvent), theEvent);
        }

        private void HandleTick(Tick obj)
        {
            log.Info("Verificando estado do serviço da Sefaz.");
            var config = configuracaoRepository.GetConfiguracao();

            if (config == null)
                return;

            if (consultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo55)
                && consultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo65))
            {
                Self.Tell(new AtivarModoOnline());
                log.Info("Modo online ativado.");
            }
            else
            {
                Self.Tell(new AtivarModoOffline("Serviço indisponível ou sem conexão com a internet", DateTime.Now));
                log.Info("Modo offline ativado.");
            }
        }

        private void HandleStart(Start obj)
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(
               TimeSpan.FromMilliseconds(0),
               TimeSpan.FromMilliseconds(3 * 60 * 1000),
               Self,
               new Tick(),
               Self);
        }
    }
}
