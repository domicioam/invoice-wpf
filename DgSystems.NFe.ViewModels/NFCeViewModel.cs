using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Input;
using EmissorNFe.Model;
using EmissorNFe.VO;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais;
using NFe.WPF.Events;
using NFe.WPF.Model;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Base;
using NFe.WPF.ViewModel.Services;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public class NFCeViewModel : ViewModelBaseValidation
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public NFCeViewModel(IDialogService dialogService, IEnviarNotaAppService enviarNotaController, INaturezaOperacaoRepository naturezaOperacaoService, IConfiguracaoService configuracaoService, IProdutoRepository produtoRepository, IDestinatarioService destinatarioService, ICertificadoRepository certificadoRepository, IEmissorService emissorService)
        {
            Pagamento = new PagamentoVO();
            Produto = new ProdutoVO();
            DestinatarioParaSalvar = new DestinatarioModel();
            TransportadoraParaSalvar = new TransportadoraModel();
            Destinatarios = new ObservableCollection<DestinatarioModel>();
            Transportadoras = new ObservableCollection<TransportadoraModel>();
            ProdutosCombo = new ObservableCollection<ProdutoEntity>();

            AdicionarProdutoCmd = new RelayCommand<object>(AdicionarProdutoCmd_Execute, null);
            GerarPagtoCmd = new RelayCommand<object>(GerarPagtoCmd_Execute, null);
            EnviarNotaCmd = new RelayCommand<IClosable>(EnviarNotaCmd_Execute);
            LoadedCmd = new RelayCommand<string>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            ExcluirProdutoNotaCmd = new RelayCommand<ProdutoVO>(ExcluirProdutoNotaCmd_Execute, null);
            ExcluirPagamentoCmd = new RelayCommand<PagamentoVO>(ExcluirPagamentoCmd_Execute, null);

            _dialogService = dialogService;
            _enviarNotaController = enviarNotaController;
            _naturezaOperacaoRepository = naturezaOperacaoService;
            _configuracaoService = configuracaoService;
            _produtoRepository = produtoRepository;
            _destinatarioService = destinatarioService;
            _certificadoRepository = certificadoRepository;
            _emissorService = emissorService;

            MessagingCenter.Subscribe<NotaFiscalMainViewModel, DestinatarioSalvoEvent>(this, nameof(DestinatarioSalvoEvent), (s, e) =>
            {
                DestinatarioVM_DestinatarioSalvoEvent(e.Destinatario);
            });

            Finalidades = new List<string>()
            {
                "Normal",
                "Complementar",
                "Ajuste",
                "Devolução"
            };

            FormasPagamento = new Dictionary<string, string>()
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

            Parcelas = new List<int>()
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
            };
        }

        private NFCeModel _notaFiscal;

        public NFCeModel NotaFiscal
        {
            get { return _notaFiscal; }
            set { SetProperty(ref _notaFiscal, value); }
        }

        #region Fields
        private PagamentoVO _pagamento;
        private ProdutoVO _produto;
        private Modelo _modelo;

        #endregion Fields

        #region Properties
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private string _busyContent;

        public string BusyContent
        {
            get { return _busyContent; }
            set { SetProperty(ref _busyContent, value); }
        }

        public DestinatarioModel DestinatarioParaSalvar { get; set; }
        public TransportadoraModel TransportadoraParaSalvar { get; set; }
        public ObservableCollection<DestinatarioModel> Destinatarios { get; set; }
        public ObservableCollection<TransportadoraModel> Transportadoras { get; set; }
        public List<string> Finalidades { get; set; }
        public PagamentoVO Pagamento
        {
            get { return _pagamento; }
            set
            {
                _pagamento = value;
                RaisePropertyChanged(nameof(Pagamento));
            }
        }

        public Dictionary<string, string> FormasPagamento { get; set; }
        public List<int> Parcelas { get; set; }

        public ProdutoVO Produto
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

        public ObservableCollection<ProdutoEntity> ProdutosCombo { get; set; }

        #endregion Properties

        #region Commands

        public ICommand SalvarTransportadoraCmd { get; set; }
        public ICommand AdicionarProdutoCmd { get; set; }
        public ICommand GerarPagtoCmd { get; set; }
        public ICommand EnviarNotaCmd { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand ClosedCmd { get; set; }
        public ICommand ExcluirProdutoNotaCmd { get; set; }
        public ICommand ExcluirPagamentoCmd { get; set; }
        #endregion Commands

        private readonly IDialogService _dialogService;
        private readonly IEnviarNotaAppService _enviarNotaController;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private readonly IConfiguracaoService _configuracaoService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IDestinatarioService _destinatarioService;
        private readonly ICertificadoRepository _certificadoRepository;
        private readonly IEmissorService _emissorService;


        private async void EnviarNotaCmd_Execute(IClosable closable)
        {
            await EnviarNota(NotaFiscal, _modelo, closable);
        }

        public async Task EnviarNota(NotaFiscalModel NotaFiscal, Modelo _modelo, IClosable closable)
        {
            NotaFiscal.ValidateModel();

            if (NotaFiscal.HasErrors)
            {
                return;
            }

            BusyContent = "Enviando...";
            IsBusy = true;
            try
            {
                X509Certificate2 certificado = _certificadoRepository.PickCertificateBasedOnInstallationType();
                var emissor = _emissorService.GetEmissor();
                var notaFiscal = await _enviarNotaController.EnviarNotaAsync(NotaFiscal, _modelo, emissor,certificado, _dialogService);
                IsBusy = false;
                var result = await _dialogService.ShowMessage("Nota enviada com sucesso! Deseja imprimi-la?", "Emissão NFe", "Sim", "Não", null);
                if (result)
                {
                    BusyContent = "Gerando impressão...";
                    IsBusy = true;
                    await _enviarNotaController.ImprimirNotaFiscal(notaFiscal);
                }
            }
            catch (ArgumentException e)
            {
                log.Error(e);
                var erro = e.Message + "\n" + e.InnerException?.Message;
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + erro, "Erro", "Ok", null);
            }
            catch (Exception e)
            {
                log.Error(e);
                var erro = e.Message + "\n" + e.InnerException?.Message;
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + erro, "Erro", "Ok", null);
            }
            finally
            {
                IsBusy = false;
                closable.Close();
            }
        }

        private void GerarPagtoCmd_Execute(object obj)
        {
            NotaFiscal.Pagamentos = NotaFiscal.Pagamentos ?? new ObservableCollection<PagamentoVO>();
            Pagamento.ValidateModel();

            if (Pagamento.HasErrors)
                return;

            NotaFiscal.Pagamentos.Add(Pagamento);
            Pagamento = new PagamentoVO();
        }

        private void AdicionarProdutoCmd_Execute(object obj)
        {
            Produto.ValidateModel();

            if (Produto.HasErrors)
                return;

            NotaFiscal.Produtos = NotaFiscal.Produtos ?? new ObservableCollection<ProdutoVO>();

            if (string.IsNullOrEmpty(NotaFiscal.NaturezaOperacao))
            {
                NaturezaOperacaoEntity naturezaOperacaoEntity = _naturezaOperacaoRepository.GetNaturezaOperacaoPorCfop(Produto.ProdutoSelecionado.GrupoImpostos.CFOP);
                NotaFiscal.NaturezaOperacao = naturezaOperacaoEntity?.Descricao;
            }

            NotaFiscal.Produtos.Add(Produto);
            Pagamento.ValorParcela += Produto.TotalLiquido;
            ProdutosCombo.Remove(Produto.ProdutoSelecionado);

            RaisePropertyChanged(nameof(ProdutosCombo));
            RaisePropertyChanged("ProdutosGrid");
            Produto = new ProdutoVO();
        }

        private void ExcluirProdutoNotaCmd_Execute(ProdutoVO produto)
        {
            NotaFiscal.Produtos.Remove(produto);
            Pagamento.ValorParcela = NotaFiscal.Produtos.Sum(p => p.TotalLiquido);
            Pagamento.FormaPagamento = "Dinheiro";
            NotaFiscal.Pagamentos?.Clear();
            ProdutosCombo.Add(produto.ProdutoSelecionado);
        }

        private void ClosedCmd_Execute()
        {
            NotaFiscal = null;
            Produto = new ProdutoVO();
            ProdutosCombo.Clear();
            Destinatarios.Clear();
        }

        private void ExcluirPagamentoCmd_Execute(PagamentoVO pagamento)
        {
            NotaFiscal.Pagamentos.Remove(pagamento);
            Pagamento.ValorParcela += pagamento.ValorParcela * pagamento.QtdeParcelas;
        }


        private void DestinatarioVM_DestinatarioSalvoEvent(DestinatarioModel destinatarioParaSalvar)
        {
            if (NotaFiscal == null)
                return;

            Destinatarios.Add(destinatarioParaSalvar);
            NotaFiscal.DestinatarioSelecionado = destinatarioParaSalvar;
        }

        private void LoadedCmd_Execute(string modelo)
        {
            NotaFiscal = new NFCeModel();

            if (modelo != null && modelo.Equals("55"))
            {
                _modelo = Modelo.Modelo55;
                NotaFiscal.IsImpressaoBobina = false;
            }
            else
            {
                _modelo = Modelo.Modelo65;
            }

            NotaFiscal.DestinatarioSelecionado = new DestinatarioModel();
            Pagamento = new PagamentoVO { FormaPagamento = "Dinheiro" };

            var config = _configuracaoService.GetConfiguracao();

            NotaFiscal.Serie = config.SerieNFCe;
            NotaFiscal.Numero = config.ProximoNumNFCe;
            NotaFiscal.ModeloNota = "NFC-e";

            NotaFiscal.DataEmissao = DateTime.Now;
            NotaFiscal.HoraEmissao = DateTime.Now;
            NotaFiscal.DataSaida = DateTime.Now;
            NotaFiscal.HoraSaida = DateTime.Now;
            NotaFiscal.IndicadorPresenca = PresencaComprador.Presencial;
            NotaFiscal.Finalidade = "Normal";

            var produtos = _produtoRepository.GetProdutosByNaturezaOperacao("Venda");
            foreach (var produto in produtos)
            {
                ProdutosCombo.Add(produto);
            }

            var destinatarios = _destinatarioService.GetAll();

            foreach (var destDb in destinatarios)
            {
                Destinatarios.Add((DestinatarioModel)destDb);
            }
        }
    }
}
