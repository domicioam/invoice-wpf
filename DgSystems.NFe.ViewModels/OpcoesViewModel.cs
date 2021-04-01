using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.WPF.Events;
using NFe.WPF.ViewModel.Base;

namespace NFe.WPF.ViewModel
{
    public class OpcoesViewModel : ViewModelBaseValidation
    {
        private string _serieNFe;
        private string _proximoNumNFe;
        private string _proximoNumNFCe;
        private string _serieNFCe;
        private string _cscID;
        private string _csc;
        private string _emailContabilidade;
        private readonly IConfiguracaoRepository _configuracaoService;

        public OpcoesViewModel(IConfiguracaoRepository configuracaoService)
        {
            SalvarCmd = new RelayCommand<Window>(SalvarCmd_Execute, null);
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _configuracaoService = configuracaoService;
        }

        [Required]
        public string SerieNFe
        {
            get
            {
                return _serieNFe;
            }
            set
            {
                SetProperty(ref _serieNFe, value);
            }
        }

        [Required]
        public string ProximoNumNFe
        {
            get
            {
                return _proximoNumNFe;
            }
            set
            {
                SetProperty(ref _proximoNumNFe, value);
            }
        }

        [Required]
        public string ProximoNumNFCe
        {
            get
            {
                return _proximoNumNFCe;
            }
            set
            {
                SetProperty(ref _proximoNumNFCe, value);
            }
        }

        [Required]
        public string SerieNFCe
        {
            get
            {
                return _serieNFCe;
            }
            set
            {
                SetProperty(ref _serieNFCe, value);
            }
        }

        [Required]
        public string CscId
        {
            get
            {
                return _cscID;
            }
            set
            {
                SetProperty(ref _cscID, value);
            }
        }

        [Required]
        public string Csc
        {
            get { return _csc; }
            set
            {
                SetProperty(ref _csc, value);
            }
        }

        [Required]
        [EmailAddress]
        public string EmailContabilidade
        {
            get { return _emailContabilidade; }
            set
            {
                SetProperty(ref _emailContabilidade, value);
            }
        }

        public ConfiguracaoEntity Configuracao { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand SalvarCmd { get; set; }

        private void LoadedCmd_Execute()
        {
            var configuracao = _configuracaoService.GetConfiguracao();

            if (configuracao == null)
                return;

            Csc = configuracao.Csc;
            CscId = configuracao.CscId;
            EmailContabilidade = configuracao.EmailContabilidade;
            ProximoNumNFCe = configuracao.ProximoNumNFCe;
            ProximoNumNFe = configuracao.ProximoNumNFe;
            SerieNFCe = configuracao.SerieNFCe;
            SerieNFe = configuracao.SerieNFe;

            Configuracao = configuracao;
        }

        private void SalvarCmd_Execute(Window wdw)
        {
            ValidateModel();

            if (!HasErrors)
            {
                ConfiguracaoEntity configuracao = null;

                if (_configuracaoService.GetConfiguracao() == null)
                {
                    configuracao = new ConfiguracaoEntity();
                }
                else
                {
                    configuracao = _configuracaoService.GetConfiguracao();
                }

                configuracao.Csc = Csc;
                configuracao.CscId = CscId;
                configuracao.EmailContabilidade = EmailContabilidade;
                configuracao.ProximoNumNFCe = ProximoNumNFCe;
                configuracao.ProximoNumNFe = ProximoNumNFe;
                configuracao.SerieNFCe = SerieNFCe;
                configuracao.SerieNFe = SerieNFe;

                _configuracaoService.Salvar(configuracao);
                Configuracao = null;

                var theEvent = new ConfiguracaoAlteradaEvent();
                MessagingCenter.Send(this, nameof(ConfiguracaoAlteradaEvent), theEvent);

                wdw.Close();
            }
        }
    }
}
