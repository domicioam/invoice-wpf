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

        private bool _isProducao;
        private ConfiguracaoEntity _configuracao;

        private string _serieNFeHom;
        private string _proximoNumNFeHom;
        private string _proximoNumNFCeHom;
        private string _serieNFCeHom;
        private string _cscIDHom;
        private string _cscHom;
        private string _emailContabilidadeHom;

        private string _serieNFeProd;
        private string _proximoNumNFeProd;
        private string _proximoNumNFCeProd;
        private string _serieNFCeProd;
        private string _cscIDProd;
        private string _cscProd;
        private string _emailContabilidadeProd;
        private IConfiguracaoService _configuracaoService;

        public bool IsProducao
        {
            get
            {
                return _isProducao;
            }
            set
            {
                SetProperty(ref _isProducao, value);
                LoadConfig(_isProducao);
            }
        }


        [Required]
        public string SerieNFeHom
        {
            get
            {
                return _serieNFeHom;
            }
            set
            {
                SetProperty(ref _serieNFeHom, value);
            }
        }

        [Required]
        public string ProximoNumNFeHom
        {
            get
            {
                return _proximoNumNFeHom;
            }
            set
            {
                SetProperty(ref _proximoNumNFeHom, value);
            }
        }

        [Required]
        public string ProximoNumNFCeHom
        {
            get
            {
                return _proximoNumNFCeHom;
            }
            set
            {
                SetProperty(ref _proximoNumNFCeHom, value);
            }
        }

        [Required]
        public string SerieNFCeHom
        {
            get
            {
                return _serieNFCeHom;
            }
            set
            {
                SetProperty(ref _serieNFCeHom, value);
            }
        }

        [Required]
        public string CscIdHom
        {
            get
            {
                return _cscIDHom;
            }
            set
            {
                SetProperty(ref _cscIDHom, value);
            }
        }

        [Required]
        public string CscHom
        {
            get { return _cscHom; }
            set
            {
                SetProperty(ref _cscHom, value);
            }
        }

        [Required]
        [EmailAddress]
        public string EmailContabilidadeHom
        {
            get { return _emailContabilidadeHom; }
            set
            {
                SetProperty(ref _emailContabilidadeHom, value);
            }
        }

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

        private void LoadConfig(bool isProducao)
        {
            _configuracao = _configuracaoService.GetConfiguracao();

            CscHom = _configuracao.CscHom;
            CscIdHom = _configuracao.CscIdHom;
            EmailContabilidadeHom = _configuracao.EmailContabilidadeHom;
            ProximoNumNFCeHom = _configuracao.ProximoNumNFCeHom;
            ProximoNumNFeHom = _configuracao.ProximoNumNFeHom;
            SerieNFCeHom = _configuracao.SerieNFCeHom;
            SerieNFeHom = _configuracao.SerieNFeHom;

            CscProd = _configuracao.Csc;
            CscIdProd = _configuracao.CscId;
            EmailContabilidadeProd = _configuracao.EmailContabilidade;
            ProximoNumNFCeProd = _configuracao.ProximoNumNFCe;
            ProximoNumNFeProd = _configuracao.ProximoNumNFe;
            SerieNFCeProd = _configuracao.SerieNFCe;
            SerieNFeProd = _configuracao.SerieNFe;
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
            IsProducao = configuracao.IsProducao;

            CscHom = configuracao.CscHom;
            CscIdHom = configuracao.CscIdHom;
            EmailContabilidadeHom = configuracao.EmailContabilidadeHom;
            ProximoNumNFCeHom = configuracao.ProximoNumNFCeHom;
            ProximoNumNFeHom = configuracao.ProximoNumNFeHom;
            SerieNFCeHom = configuracao.SerieNFCeHom;
            SerieNFeHom = configuracao.SerieNFeHom;

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

                configuracao.IsProducao = IsProducao;

                configuracao.CscHom = CscHom;
                configuracao.CscIdHom = CscIdHom;
                configuracao.EmailContabilidadeHom = EmailContabilidadeHom;
                configuracao.ProximoNumNFCeHom = ProximoNumNFCeHom;
                configuracao.ProximoNumNFeHom = ProximoNumNFeHom;
                configuracao.SerieNFCeHom = SerieNFCeHom;
                configuracao.SerieNFeHom = SerieNFeHom;

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
