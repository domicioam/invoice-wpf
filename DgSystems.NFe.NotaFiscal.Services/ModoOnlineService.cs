using Akka.Actor;
using DgSystems.NFe.Services.Actors;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NFe.Core.NotasFiscais.Services
{
    public class ModoOnlineService
    {
        private static Timer _timer;
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IConsultaStatusServicoSefazService _consultaStatusServicoService;
        private readonly IEmiteNotaFiscalContingenciaFacade _emiteNotaFiscalContingenciaService;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ActorSystem actorSystem;
        private readonly IEmitenteRepository emissorService;
        private readonly IConsultarNotaFiscalService nfeConsulta;
        private readonly IServiceFactory serviceFactory;
        private readonly ICertificadoService certificadoService;
        private readonly SefazSettings sefazSettings;

        public ModoOnlineService(IConfiguracaoRepository configuracaoRepository, IConsultaStatusServicoSefazService consultaStatusServicoService,
            INotaFiscalRepository notaFiscalRepository, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService, ActorSystem actorSystem,
            IEmitenteRepository emissorService, IConsultarNotaFiscalService nfeConsulta, IServiceFactory serviceFactory, ICertificadoService certificadoService, SefazSettings sefazSettings)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _configuracaoRepository = configuracaoRepository;
            _consultaStatusServicoService = consultaStatusServicoService;

            MessagingCenter.Subscribe<EnviarNotaFiscalService, NotaFiscalEmitidaEmContingenciaEvent>(this, nameof(NotaFiscalEmitidaEmContingenciaEvent), (s, e) =>
            {
                NotaEmitidaEmContingenciaEvent(e.justificativa, e.horário);
            });

            _emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            this.actorSystem = actorSystem;
            this.emissorService = emissorService;
            this.nfeConsulta = nfeConsulta;
            this.serviceFactory = serviceFactory;
            this.certificadoService = certificadoService;
            this.sefazSettings = sefazSettings;
        }

        private void NotaEmitidaEmContingenciaEvent(string justificativa, DateTime horário)
        {
            log.Info("Evento de nota fiscal emitida em contingência recebido.");

            AtivarModoOffline(justificativa, horário);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            log.Info("Verificando estado do serviço da Sefaz.");
            var config = _configuracaoRepository.GetConfiguracao();

            if (config == null)
                return;

            if (_consultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo55)
                && _consultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo65))
            {
                if (!config.IsContingencia) return;

                AtivarModoOnline();
                log.Info("Modo online ativado.");
            }
            else
            {
                if (config.IsContingencia) return;

                AtivarModoOffline("Serviço indisponível ou sem conexão com a internet", DateTime.Now);
                log.Info("Modo offline ativado.");
            }
        }

        public async void AtivarModoOnline()
        {
            var configuração = _configuracaoRepository.GetConfiguracao();
            var dataHoraContingencia = configuração.DataHoraEntradaContingencia;
            var primeiraNotaContingencia =
                _notaFiscalRepository.GetPrimeiraNotaEmitidaEmContingencia(dataHoraContingencia, DateTime.Now);

            NotaFiscalEntity notaParaCancelar = null;

            if (primeiraNotaContingencia != null)
            {
                var numero = int.Parse(primeiraNotaContingencia.Numero) - 1;
                notaParaCancelar = _notaFiscalRepository.GetNota(numero.ToString(), primeiraNotaContingencia.Serie,
                    primeiraNotaContingencia.Modelo);
            }

            var emiteNfeContingenciaActor = actorSystem.ActorOf(Props.Create(() => new EmiteNFeContingenciaActor(_notaFiscalRepository, emissorService, nfeConsulta, serviceFactory, certificadoService, sefazSettings)));

            try
            {
                var result = await emiteNfeContingenciaActor.Ask<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia(), TimeSpan.FromSeconds(30));

                if (result.Erros != null)
                {
                    _emiteNotaFiscalContingenciaService.InutilizarCancelarNotasPendentesContingencia(notaParaCancelar,
                        _notaFiscalRepository);

                    var theEvent = new NotasFiscaisTransmitidasEvent() { MensagensErro = result.Erros };
                    MessagingCenter.Send(this, nameof(NotasFiscaisTransmitidasEvent), theEvent);
                }

                configuração.IsContingencia = false;
                _configuracaoRepository.Salvar(configuração);
            }catch (Exception e)
            {
                var theEvent = new NotasFiscaisTransmitidasEvent() { MensagensErro = new List<string> { "Erro ao tentar transmitir notas emitidas em contingência." } };
                MessagingCenter.Send(this, nameof(NotasFiscaisTransmitidasEvent), theEvent);
                log.Error("Erro ao tentar transmitir as notas emitidas em contingência.", e);
            }
            finally
            {
                actorSystem.Stop(emiteNfeContingenciaActor);
            }
        }

        public void AtivarModoOffline(string justificativa, DateTime dataHoraContingencia)
        {
            var config = _configuracaoRepository.GetConfiguracao();
            config.IsContingencia = true;
            config.DataHoraEntradaContingencia = dataHoraContingencia;
            config.JustificativaContingencia = justificativa;
            _configuracaoRepository.Salvar(config);

            var theEvent = new ServicoOfflineEvent();
            MessagingCenter.Send(this, nameof(ServicoOfflineEvent), theEvent);
        }

        public void StartTimer()
        {
            _timer = new Timer();

            _timer.Tick += Timer_Tick;
            _timer.Interval = 3 * 60 * 1000;
            _timer.Start();
            Timer_Tick(null, EventArgs.Empty);
        }
    }
}