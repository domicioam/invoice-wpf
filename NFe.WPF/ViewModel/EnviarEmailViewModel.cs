using GalaSoft.MvvmLight.CommandWpf;
using NFe.Core.Utils;
using NFe.Repository;
using NFe.Repository.Repositories;
using NFe.WPF.View.NotaFiscal;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NFe.Core.Cadastro.Configuracoes;
using NFe.WPF.ViewModel.Base;
using NFe.Core.Interfaces;
using NFe.WPF.Utils;
using NFe.WPF.Commands;
using MediatR;
using System.Threading;

namespace NFe.WPF.ViewModel
{
    public class EnviarEmailViewModel : ViewModelBaseValidation, IRequestHandler<EnviarEmailCommand>
    {
        public EnviarEmailViewModel(MailManager mailManager, IConfiguracaoService configuracaoService, INotaFiscalRepository notaFiscalRepository)
        {
            EnviarEmailCmd = new RelayCommand<IClosable>(EnviarEmailCmd_Execute, EnviarEmailCmd_CanExecute);
            _mailManager = mailManager;
            _configuracaoService = configuracaoService;
            _notaFiscalRepository = notaFiscalRepository;
        }

        public string Chave { get; set; }

        private string _email;
        private MailManager _mailManager;
        private IConfiguracaoService _configuracaoService;
        private INotaFiscalRepository _notaFiscalRepository;

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

        public ICommand EnviarEmailCmd { get; set; }

        private bool EnviarEmailCmd_CanExecute(IClosable closable)
        {
            ValidateModel(); //valida e-mail, mudar para validate property, caso necessário
            return !HasErrors && !string.IsNullOrWhiteSpace(Email);
        }

        internal void EnviarEmail(string chave)
        {
            Chave = chave;
            var app = Application.Current;
            var mainWindow = app.MainWindow;
            var window = new EnviarEmailWindow() { Owner = mainWindow };
            window.ShowDialog();
        }

        private async void EnviarEmailCmd_Execute(IClosable closable)
        {
            var config = await _configuracaoService.GetConfiguracaoAsync();
            var notaFiscal =  _notaFiscalRepository.GetNotaFiscalByChave(Chave);
            string xmlPath = notaFiscal.XmlPath;
            _mailManager.EnviarEmailDestinatario(Email, xmlPath, notaFiscal);
            Email = string.Empty;
            closable.Close();
        }

        public Task<Unit> Handle(EnviarEmailCommand request, CancellationToken cancellationToken)
        {
            EnviarEmail(request.Chave);
            return Unit.Task;
        }
    }
}
