using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System;
using System.Globalization;
using System.Linq;
using NFe.Core.Utils.Zip;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using NFe.Repository.Repositories;
using NFe.Core.Utils;
using NFe.Core.NotasFiscais.Services;
using NFe.WPF.Utils;

namespace EmissorNFe.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const int diaParaEnvio = 2;
        private MailManager _mailManager;
        private ModoOnlineService _modoOnlineService;

        public ICommand LoadedCmd { get; set; }

        private void LoadedCmd_Execute()
        {
            Task.Run(() =>
            {
                _modoOnlineService.StartTimer();
                _mailManager.EnviarNotasParaContabilidade(diaParaEnvio);
            });
        }

        public MainViewModel(ModoOnlineService modoOnlineService, MailManager mailManager)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _modoOnlineService = modoOnlineService;
            _mailManager = mailManager;
        }
    }
}