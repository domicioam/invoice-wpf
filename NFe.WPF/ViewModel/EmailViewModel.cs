using EmissorNFe.ViewModel.Base;
using GalaSoft.MvvmLight.CommandWpf;
using NFe.Core.Domain.Services.Configuracao;
using NFe.Core.Utils;
using NFe.Repository;
using NFe.Repository.Repositories;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NFe.WPF.ViewModel
{
    public class EmailViewModel : ViewModelBaseValidation
    {
        public EmailViewModel()
        {
            EnviarEmailCmd = new RelayCommand<IClosable>(EnviarEmailCmd_Execute, EnviarEmailCmd_CanExecute);

        }

        public ICommand EnviarEmailCmd { get; set; }

        public string ChaveNotaSelecionada { get; set; }

        private string _email;

        [EmailAddress]
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                EnviarEmailCmd_CanExecute(null);
            }
        }

        private bool EnviarEmailCmd_CanExecute(IClosable closable)
        {
            ValidateModel(); //valida e-mail, mudar para validate property, caso necessário
            return !HasErrors && !string.IsNullOrWhiteSpace(Email);
        }

        private async void EnviarEmailCmd_Execute(IClosable closable)
        {
            var notaFiscalRepository = new NotaFiscalRepository(new NFeContext());
            var config = await ConfiguracaoService.GetConfiguracaoAsync();
            var notaFiscal = await notaFiscalRepository.GetNotaFiscalByChaveAsync(ChaveNotaSelecionada, config.IsProducao ? 1 : 2);
            string xmlPath = notaFiscal.XmlPath;
            await MailManager.EnviarEmailDestinatario(Email, xmlPath);
            Email = string.Empty;
            closable.Close();
        }
    }
}
