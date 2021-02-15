using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NFe.Core.NotasFiscais.Services;
using NFe.WPF.Utils;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmissorNFe.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const int diaParaEnvio = 2;
        private readonly MailManager _mailManager;
        private readonly ModoOnlineService _modoOnlineService;

        public ICommand LoadedCmd { get; set; }

        private void LoadedCmd_Execute()
        {
            Task.Run(() =>
            {
                _modoOnlineService.StartTimer();
            });

            _mailManager.EnviarNotasParaContabilidade(diaParaEnvio);
        }

        public MainViewModel(ModoOnlineService modoOnlineService, MailManager mailManager)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _modoOnlineService = modoOnlineService;
            _mailManager = mailManager;
        }
    }
}