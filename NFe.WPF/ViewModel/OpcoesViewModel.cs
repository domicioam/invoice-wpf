using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Configuracoes;
using NFe.WPF.ViewModel.Base;

namespace NFe.WPF.ViewModel
{
    public delegate void ConfiguracaoAlteradaEventHandler();

    public class OpcoesViewModel : ViewModelBaseValidation
    {
        public ConfiguracaoAlteradaEventHandler ConfiguracaoAlteradaEvent = delegate { };

        private ConfiguracaoEntity _configuracao;

        private string _serieNFeProd;
        private string _proximoNumNFeProd;
        private string _proximoNumNFCeProd;
        private string _serieNFCeProd;
        private string _cscIDProd;
        private string _cscProd;
        private string _emailContabilidadeProd;
        private IConfiguracaoService _configuracaoService;

        [Required]
        public string SerieNFeProd
        {
            get
            {
                return _serieNFeProd;
            }
            set
            {
                SetProperty(ref _serieNFeProd, value);
            }
        }

        [Required]
        public string ProximoNumNFeProd
        {
            get
            {
                return _proximoNumNFeProd;
            }
            set
            {
                SetProperty(ref _proximoNumNFeProd, value);
            }
        }

        [Required]
        public string ProximoNumNFCeProd
        {
            get
            {
                return _proximoNumNFCeProd;
            }
            set
            {
                SetProperty(ref _proximoNumNFCeProd, value);
            }
        }

        [Required]
        public string SerieNFCeProd
        {
            get
            {
                return _serieNFCeProd;
            }
            set
            {
                SetProperty(ref _serieNFCeProd, value);
            }
        }

        [Required]
        public string CscIdProd
        {
            get
            {
                return _cscIDProd;
            }
            set
            {
                SetProperty(ref _cscIDProd, value);
            }
        }

        [Required]
        public string CscProd
        {
            get { return _cscProd; }
            set
            {
                SetProperty(ref _cscProd, value);
            }
        }

        [Required]
        [EmailAddress]
        public string EmailContabilidadeProd
        {
            get { return _emailContabilidadeProd; }
            set
            {
                SetProperty(ref _emailContabilidadeProd, value);
            }
        }

        public ICommand SalvarCmd { get; set; }
        public ICommand LoadedCmd { get; set; }

        public OpcoesViewModel(IConfiguracaoService configuracaoService)
        {
            SalvarCmd = new RelayCommand<Window>(SalvarCmd_Execute, null);
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _configuracaoService = configuracaoService;
        }

        private void LoadedCmd_Execute()
        {
            var configuracao = _configuracaoService.GetConfiguracao();

            if (configuracao == null)
                return;

            CscProd = configuracao.Csc;
            CscIdProd = configuracao.CscId;
            EmailContabilidadeProd = configuracao.EmailContabilidade;
            ProximoNumNFCeProd = configuracao.ProximoNumNFCe;
            ProximoNumNFeProd = configuracao.ProximoNumNFe;
            SerieNFCeProd = configuracao.SerieNFCe;
            SerieNFeProd = configuracao.SerieNFe;

            _configuracao = configuracao;
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

                configuracao.Csc = CscProd;
                configuracao.CscId = CscIdProd;
                configuracao.EmailContabilidade = EmailContabilidadeProd;
                configuracao.ProximoNumNFCe = ProximoNumNFCeProd;
                configuracao.ProximoNumNFe = ProximoNumNFeProd;
                configuracao.SerieNFCe = SerieNFCeProd;
                configuracao.SerieNFe = SerieNFeProd;

                _configuracaoService.Salvar(configuracao);
                _configuracao = null;
                ConfiguracaoAlteradaEvent();
                wdw.Close();
            }
        }
    }
}
