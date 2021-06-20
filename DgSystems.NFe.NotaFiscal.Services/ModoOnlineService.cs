using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.Sefaz.Facades;
using System;
using System.Windows.Forms;

namespace NFe.Core.NotasFiscais.Services
{
    public class ModoOnlineService
    {
        private static Timer _timer;
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IConsultaStatusServicoSefazService _consultaStatusServicoService;
        private bool _isOnline;
        private readonly IEmiteNotaFiscalContingenciaFacade _emiteNotaFiscalContingenciaService;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string Name => nameof(ModoOnlineService);


        public ModoOnlineService(IConfiguracaoRepository configuracaoRepository, IConsultaStatusServicoSefazService consultaStatusServicoService,
            INotaFiscalRepository notaFiscalRepository, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _configuracaoRepository = configuracaoRepository;
            _consultaStatusServicoService = consultaStatusServicoService;

            MessagingCenter.Subscribe<EnviarNotaFiscalService, NotaFiscalEmitidaEmContingenciaEvent>(this, nameof(NotaFiscalEmitidaEmContingenciaEvent), (s, e) =>
            {
                NotaEmitidaEmContingenciaEvent(e.justificativa, e.horário);
            });

            _emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
        }

        private void NotaEmitidaEmContingenciaEvent(string justificativa, DateTime horário)
        {
            log.Info($"{Name}: Evento de nota fiscal emitida em contingência recebido.");

            AtivarModoOffline(justificativa, horário);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            log.Info($"{Name}: Verificando estado do serviço da Sefaz.");
            var config = _configuracaoRepository.GetConfiguracao();

            if (config == null)
                return;

            if (_consultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo55)
                && _consultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo65))
            {
                if (_isOnline) return;

                AtivarModoOnline();
                _isOnline = true;
            }
            else
            {
                if (!_isOnline) return;

                AtivarModoOffline("Serviço indisponível ou sem conexão com a internet",
                    DateTime.Now);
                _isOnline = false;
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

            var mensagensErro = await _emiteNotaFiscalContingenciaService.TransmitirNotasFiscalEmContingencia();

            if (mensagensErro != null)
            {
                _emiteNotaFiscalContingenciaService.InutilizarCancelarNotasPendentesContingencia(notaParaCancelar,
                    _notaFiscalRepository);

                var theEvent = new NotasFiscaisTransmitidasEvent() { MensagensErro = mensagensErro };
                MessagingCenter.Send(this, nameof(NotasFiscaisTransmitidasEvent), theEvent);
            }

            configuração.IsContingencia = false;
            _configuracaoRepository.Salvar(configuração);

            log.Info($"{Name}: Modo online ativado.");
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

            log.Info($"{Name}: Modo offline ativado.");
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