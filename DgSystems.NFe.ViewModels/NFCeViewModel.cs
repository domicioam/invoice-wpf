using EmissorNFe.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.WPF.Events;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Input;
using NFeCoreModelo = NFe.Core.Domain.Modelo;

namespace DgSystems.NFe.ViewModels
{
    public class NFCeViewModel : NotaFiscalModel
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public NFCeViewModel(IDialogService dialogService, IEnviarNotaAppService enviarNotaAppService, INaturezaOperacaoRepository naturezaOperacaoService, IConfiguracaoRepository configuracaoService, IProdutoRepository produtoRepository, IDestinatarioRepository destinatarioService, ICertificadoService certificadoRepository, IEmitenteRepository emissorService)
        {
            Pagamento = new PagamentoModel();
            Produto = new ProdutoModel();
            DestinatarioParaSalvar = new DestinatarioModel();
            TransportadoraParaSalvar = new TransportadoraModel();
            Destinatarios = new ObservableCollection<DestinatarioModel>();
            Transportadoras = new ObservableCollection<TransportadoraModel>();
            ProdutosCombo = new ObservableCollection<ProdutoEntity>();

            AdicionarProdutoCmd = new RelayCommand<object>(AdicionarProdutoCmd_Execute, null);
            GerarPagtoCmd = new RelayCommand<object>(GerarPagtoCmd_Execute, null);
            EnviarNotaCmd = new RelayCommand<IClosable>(EnviarNotaCmd_ExecuteAsync);
            LoadedCmd = new RelayCommand<string>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            ExcluirProdutoNotaCmd = new RelayCommand<ProdutoModel>(ExcluirProdutoNotaCmd_Execute, null);
            ExcluirPagamentoCmd = new RelayCommand<PagamentoModel>(ExcluirPagamentoCmd_Execute, null);

            _dialogService = dialogService;
            _enviarNotaAppService = enviarNotaAppService;
            _naturezaOperacaoRepository = naturezaOperacaoService;
            _configuracaoRepository = configuracaoService;
            _produtoRepository = produtoRepository;
            _destinatarioService = destinatarioService;
            _certificadoRepository = certificadoRepository;
            _emissorService = emissorService;

            MessagingCenter.Subscribe<NotaFiscalMainViewModel, DestinatarioSalvoEvent>(this, nameof(DestinatarioSalvoEvent), (_, e) => DestinatarioVM_DestinatarioSalvoEvent(e.Destinatario));
        }

        private PagamentoModel _pagamento;
        private ProdutoModel _produto;
        private Modelo _modelo;
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
        public List<string> Finalidades => new List<string>()
        {
            "Normal",
            "Complementar",
            "Ajuste",
            "Devolução"
        };
        public PagamentoModel Pagamento
        {
            get { return _pagamento; }
            set
            {
                _pagamento = value;
                RaisePropertyChanged(nameof(Pagamento));
            }
        }

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

        public ObservableCollection<ProdutoEntity> ProdutosCombo { get; set; }

        public ICommand SalvarTransportadoraCmd { get; set; }
        public ICommand AdicionarProdutoCmd { get; set; }
        public ICommand GerarPagtoCmd { get; set; }
        public ICommand EnviarNotaCmd { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand ClosedCmd { get; set; }
        public ICommand ExcluirProdutoNotaCmd { get; set; }
        public ICommand ExcluirPagamentoCmd { get; set; }

        private readonly IDialogService _dialogService;
        private readonly IEnviarNotaAppService _enviarNotaAppService;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IDestinatarioRepository _destinatarioService;
        private readonly ICertificadoService _certificadoRepository;
        private readonly IEmitenteRepository _emissorService;


        private async void EnviarNotaCmd_ExecuteAsync(IClosable closable)
        {
            await EnviarNotaAsync(closable);
        }

        private async Task EnviarNotaAsync(IClosable closable)
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
                var notaFiscal = await _enviarNotaAppService.EnviarNotaAsync(this, _modelo, emissor, certificado, _dialogService);
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
            Pagamentos = Pagamentos ?? new ObservableCollection<PagamentoModel>();
            Pagamento.ValidateModel();

            if (Pagamento.HasErrors)
                return;

            Pagamentos.Add(Pagamento);
            Pagamento = new PagamentoModel();
        }

        private void AdicionarProdutoCmd_Execute(object obj)
        {
            Produto.ValidateModel();

            if (Produto.HasErrors)
                return;

            Produtos = Produtos ?? new ObservableCollection<ProdutoModel>();

            if (string.IsNullOrEmpty(NaturezaOperacao))
            {
                NaturezaOperacaoEntity naturezaOperacaoEntity = _naturezaOperacaoRepository.GetNaturezaOperacaoPorCfop(Produto.ProdutoSelecionado.GrupoImpostos.CFOP);
                NaturezaOperacao = naturezaOperacaoEntity?.Descricao;
            }

            Produtos.Add(Produto);
            Pagamento.ValorParcela += Produto.TotalLiquido;
            ProdutosCombo.Remove(Produto.ProdutoSelecionado);

            RaisePropertyChanged(nameof(ProdutosCombo));
            RaisePropertyChanged("ProdutosGrid");
            Produto = new ProdutoModel();
        }

        private void ExcluirProdutoNotaCmd_Execute(ProdutoModel produto)
        {
            Produtos.Remove(produto);
            Pagamento.ValorParcela = Produtos.Sum(p => p.TotalLiquido);
            Pagamento.FormaPagamento = "Dinheiro";
            Pagamentos?.Clear();
            ProdutosCombo.Add(produto.ProdutoSelecionado);
        }

        private void ClosedCmd_Execute()
        {
            Produto = new ProdutoModel();
            ProdutosCombo.Clear();
            Destinatarios.Clear();
        }

        private void ExcluirPagamentoCmd_Execute(PagamentoModel pagamento)
        {
            Pagamentos.Remove(pagamento);
            Pagamento.ValorParcela += pagamento.ValorParcela * pagamento.QtdeParcelas;
        }

        private void DestinatarioVM_DestinatarioSalvoEvent(DestinatarioModel destinatarioParaSalvar)
        {
            Destinatarios.Add(destinatarioParaSalvar);
            DestinatarioSelecionado = destinatarioParaSalvar;
        }

        private void LoadedCmd_Execute(string modelo)
        {
            if (modelo?.Equals("55") == true)
            {
                _modelo = NFeCoreModelo.Modelo55;
                IsImpressaoBobina = false;
            }
            else
            {
                _modelo = NFeCoreModelo.Modelo65;
            }

            DestinatarioSelecionado = new DestinatarioModel();
            Pagamento = new PagamentoModel { FormaPagamento = "Dinheiro" };

            var config = _configuracaoRepository.GetConfiguracao();

            Serie = config.SerieNFCe;
            Numero = config.ProximoNumNFCe;
            ModeloNota = "NFC-e";

            DataEmissao = DateTime.Now;
            HoraEmissao = DateTime.Now;
            DataSaida = DateTime.Now;
            HoraSaida = DateTime.Now;
            IndicadorPresenca = PresencaComprador.Presencial;
            Finalidade = "Normal";

            var produtos = _produtoRepository.GetProdutosByNaturezaOperacao("Venda");
            foreach (var produto in produtos)
            {
                ProdutosCombo.Add(produto);
            }

            foreach (var destDb in _destinatarioService.GetAll())
            {
                Destinatarios.Add((DestinatarioModel)destDb);
            }
        }

        private string _chave;
        private string _protocolo;

        public string Chave
        {
            get { return _chave; }
            set { SetProperty(ref _chave, value); }
        }

        public string Protocolo
        {
            get { return _protocolo; }
            set { SetProperty(ref _protocolo, value); }
        }

        public string Status { get; set; }
        public bool IsCancelada { get; internal set; }
    }
}
