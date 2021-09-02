using Akka.Actor;
using DgSystems.NFe.Services.Actors;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.WPF.Utils;
using System.Globalization;
using System.Windows.Input;

namespace EmissorNFe.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const int diaParaEnvio = 2;
        private readonly MailManager _mailManager;
        private readonly IConfiguracaoRepository configuracaoRepository;
        private readonly IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService;
        private readonly IConsultarNotaFiscalService nfeConsulta;

        public ICommand LoadedCmd { get; }
        public INotaFiscalRepository NotaFiscalRepository { get; }

        private readonly IEmitenteRepository emissorService;
        private readonly IServiceFactory serviceFactory;
        private readonly CertificadoService certificadoService;
        private readonly ActorSystem actorSystem;
        private readonly IConsultaStatusServicoSefazService consultaStatusServicoService;
        private readonly SefazSettings sefazSettings;

        private void LoadedCmd_Execute()
        {
            var modoOnlineActor = actorSystem.ActorOf(Props.Create(() => new ModoOnlineActor(configuracaoRepository, consultaStatusServicoService, NotaFiscalRepository, emiteNotaFiscalContingenciaService, emissorService, nfeConsulta, serviceFactory, certificadoService, sefazSettings)), "modoOnline");
            modoOnlineActor.Tell(new ModoOnlineActor.Start());

            _mailManager.EnviarNotasParaContabilidade(diaParaEnvio);
        }

        public MainViewModel(MailManager mailManager, IConfiguracaoRepository configuracaoRepository, IConsultaStatusServicoSefazService consultaStatusServicoService,
            INotaFiscalRepository notaFiscalRepository, IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService, ActorSystem actorSystem,
            IEmitenteRepository emissorService, IConsultarNotaFiscalService nfeConsulta, IServiceFactory serviceFactory, CertificadoService certificadoService, SefazSettings sefazSettings)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _mailManager = mailManager;
            this.configuracaoRepository = configuracaoRepository;
            NotaFiscalRepository = notaFiscalRepository;
            this.emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            this.nfeConsulta = nfeConsulta;
            this.emissorService = emissorService;
            this.serviceFactory = serviceFactory;
            this.certificadoService = certificadoService;
            this.actorSystem = actorSystem;
            this.consultaStatusServicoService = consultaStatusServicoService;
            this.sefazSettings = sefazSettings;
        }
    }
}