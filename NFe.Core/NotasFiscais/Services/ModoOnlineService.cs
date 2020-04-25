using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.NotasFiscais.Services
{
    public delegate void ServicoOfflineEventHandler();

    public delegate void NotasTransmitidasEventHandler(List<string> mensagensErro);

    public class ModoOnlineService
    {
        private static Timer _timer;
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IConsultaStatusServicoFacade _consultaStatusServicoService;
        private bool _isOnline;
        private readonly IEmiteNotaFiscalContingenciaFacade _emiteNotaFiscalContingenciaService;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        public ModoOnlineService(IEnviaNotaFiscalFacade enviaNotaFiscalService,
            IConfiguracaoRepository configuracaoRepository, IConsultaStatusServicoFacade consultaStatusServicoService,
            INotaFiscalRepository notaFiscalRepository, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _configuracaoRepository = configuracaoRepository;
            _consultaStatusServicoService = consultaStatusServicoService;

            enviaNotaFiscalService.NotaEmitidaEmContingenciaEvent +=
                EnviaNotaFiscalServiceEnviaNotaEmitidaEmContingenciaEvent;
            _emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
        }

        public event ServicoOfflineEventHandler ServicoOfflineEvent = delegate { };
        public event NotasTransmitidasEventHandler NotasTransmitidasEvent = delegate { };

        private void EnviaNotaFiscalServiceEnviaNotaEmitidaEmContingenciaEvent(string justificativa, DateTime horário)
        {
            AtivarModoOffline(justificativa, horário);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
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
                NotasTransmitidasEvent(mensagensErro);
            }

            configuração.IsContingencia = false;
            _configuracaoRepository.Salvar(configuração);
        }

        public void AtivarModoOffline(string justificativa, DateTime dataHoraContingencia)
        {
            var config = _configuracaoRepository.GetConfiguracao();
            config.IsContingencia = true;
            config.DataHoraEntradaContingencia = dataHoraContingencia;
            config.JustificativaContingencia = justificativa;
            _configuracaoRepository.Salvar(config);

            ServicoOfflineEvent();
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