using DgSystems.NFe.ViewModels;
using EmissorNFe.Model;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.ViewModel.Base;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Input;
using NFeCoreModelo = NFe.Core.Domain.Modelo;

namespace NFe.WPF.NotaFiscal.Model
{
    public abstract class NotaFiscalModel : ViewModelBaseValidation
    {
        private string _dataAutorizacao;
        private DateTime _dataEmissao;
        private DateTime _dataSaida;
        private DestinatarioModel _destinatarioSelecionado;
        private string _documento;
        private string _finalidade;
        private DateTime _horaEmissao;
        private DateTime _horaSaida;
        private PresencaComprador _indicadorPresenca;
        private bool _isCpfCnpjFieldEnabled;
        private bool _isEstrangeiro;
        private bool _isImpressaoBobina = true;
        private string _modelo;
        private string _modeloNota;
        private string _naturezaOperacao;
        private string _numero;
        private ObservableCollection<PagamentoModel> _pagamentos;
        private ObservableCollection<ProdutoModel> _produtos;
        private string _serie;
        private ProdutoModel _produto;
        private PagamentoModel _pagamento;
        private bool _isBusy;
        private string _busyContent;
        private ICertificadoService _certificadoRepository;
        private IEnviarNotaAppService _enviarNotaAppService;
        protected IDialogService _dialogService;
        protected IEmitenteRepository _emissorService;
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Destinatario { get; set; }
        public string UfDestinatario { get; set; }
        public string Valor { get; set; }

        public bool IsEstrangeiro
        {
            get { return _isEstrangeiro; }
            set { SetProperty(ref _isEstrangeiro, value); }
        }

        [Required]
        public ObservableCollection<ProdutoModel> Produtos
        {
            get { return _produtos; }
            set { SetProperty(ref _produtos, value); }
        }

        public bool IsCpfCnpjFieldEnabled
        {
            get { return _isCpfCnpjFieldEnabled; }
            set { SetProperty(ref _isCpfCnpjFieldEnabled, value); }
        }

        public DestinatarioModel DestinatarioSelecionado
        {
            get { return _destinatarioSelecionado; }
            set
            {
                SetProperty(ref _destinatarioSelecionado, value);
                IsCpfCnpjFieldEnabled = string.IsNullOrEmpty(value.NomeRazao);

                switch (value.TipoDestinatario)
                {
                    case TipoDestinatario.PessoaFisica:
                        Documento = value.CPF;
                        IsEstrangeiro = false;
                        break;
                    case TipoDestinatario.PessoaJuridica:
                        Documento = value.CNPJ;
                        IsEstrangeiro = false;
                        break;
                    case TipoDestinatario.Estrangeiro:
                        Documento = value.IdEstrangeiro;
                        IsEstrangeiro = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [Required]
        public ObservableCollection<PagamentoModel> Pagamentos
        {
            get { return _pagamentos; }
            set { SetProperty(ref _pagamentos, value); }
        }

        public string Serie
        {
            get { return _serie; }
            set { SetProperty(ref _serie, value); }
        }

        public string Numero
        {
            get { return _numero; }
            set { SetProperty(ref _numero, value); }
        }

        public string Documento
        {
            get { return _documento; }
            set { SetProperty(ref _documento, value); }
        }

        [Required]
        public string NaturezaOperacao
        {
            get { return _naturezaOperacao; }
            set { SetProperty(ref _naturezaOperacao, value); }
        }

        [Required]
        public DateTime DataEmissao
        {
            get { return _dataEmissao; }
            set { SetProperty(ref _dataEmissao, value); }
        }

        [Required]
        public DateTime HoraEmissao
        {
            get { return _horaEmissao; }
            set { SetProperty(ref _horaEmissao, value); }
        }

        [Required]
        public DateTime DataSaida
        {
            get { return _dataSaida; }
            set { SetProperty(ref _dataSaida, value); }
        }

        [Required]
        public DateTime HoraSaida
        {
            get { return _horaSaida; }
            set { SetProperty(ref _horaSaida, value); }
        }

        public PresencaComprador IndicadorPresenca
        {
            get { return _indicadorPresenca; }
            set { SetProperty(ref _indicadorPresenca, value); }
        }

        public string Finalidade
        {
            get { return _finalidade; }
            set { SetProperty(ref _finalidade, value); }
        }

        public string Modelo
        {
            get { return !_modelo.Equals("55") ? _modelo.Equals("65") ? "NFC-e" : null : "NF-e"; }
            set { SetProperty(ref _modelo, value); }
        }

        public string ModeloNota
        {
            get { return _modeloNota; }
            set { SetProperty(ref _modeloNota, value); }
        }

        public bool IsImpressaoBobina
        {
            get { return _isImpressaoBobina; }
            set { SetProperty(ref _isImpressaoBobina, value); }
        }

        public string DataAutorizacao
        {
            get
            {
                if (string.IsNullOrEmpty(_dataAutorizacao))
                    return null;

                if (DateTime.TryParseExact(_dataAutorizacao, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date)
                    || DateTime.TryParseExact(_dataAutorizacao, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out date))
                    return date.ToString("dd/MM/yyyy HH:mm:ss");

                return null;
            }
            set { SetProperty(ref _dataAutorizacao, value); }
        }
        public PagamentoModel Pagamento
        {
            get { return _pagamento; }
            set
            {
                _pagamento = value;
                RaisePropertyChanged(nameof(Pagamento));
            }
        }
        public List<string> Finalidades => new List<string>()
        {
            "Normal",
            "Complementar",
            "Ajuste",
            "Devolução"
        };

        public Dictionary<string, string> FormasPagamento => new Dictionary<string, string>()
        {
            { "Dinheiro", "Dinheiro" },
            { "Cheque", "Cheque" },
            { "CartaoCredito", "Cartão de Crédito" },
            { "CartaoDebito", "Cartão de Débito" }
            //{ "CreditoLoja", "Crédito Loja" },
            //{ "ValeAlimentacao",  "Vale Alimentação" },
            //{ "ValeRefeicao", "Vale Refeição" },
            //{ "ValePresente", "Vale Presente"},
            //{ "ValeCombustivel", "Vale Combustível"},
            //{ "Outros", "Outros" }
        };

        public List<int> Parcelas => new List<int>()
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
        };

        public ProdutoModel Produto
        {
            get
            {
                return _produto;
            }
            set
            {
                SetProperty(ref _produto, value);
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public string BusyContent
        {
            get { return _busyContent; }
            set { SetProperty(ref _busyContent, value); }
        }

        public DestinatarioModel DestinatarioParaSalvar { get; set; }

        public TransportadoraModel TransportadoraParaSalvar { get; set; }

        public ICommand SalvarTransportadoraCmd { get; set; }
        public ICommand AdicionarProdutoCmd { get; set; }
        public ICommand GerarPagtoCmd { get; set; }
        public ICommand EnviarNotaCmd { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand ClosedCmd { get; set; }
        public ICommand ExcluirProdutoNotaCmd { get; set; }
        public ICommand ExcluirPagamentoCmd { get; set; }
        public NFeCoreModelo Modelo1 { get; set; }

        protected virtual async Task EnviarNotaAsync(IClosable closable)
        {
            ValidateModel();

            if (HasErrors)
            {
                return;
            }

            BusyContent = "Enviando...";
            IsBusy = true;

            try
            {
                X509Certificate2 certificado = _certificadoRepository.GetX509Certificate2();
                var emissor = _emissorService.GetEmissor();
                var notaFiscal = await _enviarNotaAppService.EnviarNotaAsync(this, Modelo1, emissor, certificado, _dialogService);
                IsBusy = false;
                var result = await _dialogService.ShowMessage("Nota enviada com sucesso! Deseja imprimi-la?", "Emissão NFe", "Sim", "Não", null);
                if (result)
                {
                    BusyContent = "Gerando impressão...";
                    IsBusy = true;
                    await _enviarNotaAppService.ImprimirNotaFiscal(notaFiscal);
                }
            }
            catch (ArgumentException e)
            {
                Log.Error(e);
                var erro = e.Message + "\n" + e.InnerException?.Message;
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + erro, "Erro", "Ok", null);
            }
            catch (Exception e)
            {
                Log.Error(e);
                var erro = e.Message + "\n" + e.InnerException?.Message;
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + erro, "Erro", "Ok", null);
            }
            finally
            {
                IsBusy = false;
                closable.Close();
            }
        }

        protected NotaFiscalModel(IEnviarNotaAppService enviarNotaAppService, IDialogService dialogService, IEmitenteRepository emissorService, ICertificadoService certificadoRepository)
        {
            _certificadoRepository = certificadoRepository;
            _enviarNotaAppService = enviarNotaAppService;
            _dialogService = dialogService;
            _emissorService = emissorService;
        }
    }
}