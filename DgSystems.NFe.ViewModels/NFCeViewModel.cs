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
using System.Collections.ObjectModel;
using System.Linq;
using NFeCoreModelo = NFe.Core.Domain.Modelo;

namespace DgSystems.NFe.ViewModels
{
    public class NFCeViewModel : NotaFiscalModel
    {
        public NFCeViewModel(IDialogService dialogService, IEnviarNotaAppService enviarNotaAppService, INaturezaOperacaoRepository naturezaOperacaoService, IConfiguracaoRepository configuracaoService, IProdutoRepository produtoRepository, IDestinatarioRepository destinatarioService, ICertificadoService certificadoRepository, IEmitenteRepository emissorService) : base(enviarNotaAppService, dialogService, emissorService, certificadoRepository)
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

            _naturezaOperacaoRepository = naturezaOperacaoService;
            _configuracaoRepository = configuracaoService;
            _produtoRepository = produtoRepository;
            _destinatarioService = destinatarioService;

            MessagingCenter.Subscribe<NotaFiscalMainViewModel, DestinatarioSalvoEvent>(this, nameof(DestinatarioSalvoEvent), (_, e) => DestinatarioVM_DestinatarioSalvoEvent(e.Destinatario));
        }

        public ObservableCollection<DestinatarioModel> Destinatarios { get; set; }
        public ObservableCollection<TransportadoraModel> Transportadoras { get; set; }

        public ObservableCollection<ProdutoEntity> ProdutosCombo { get; set; }

        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IDestinatarioRepository _destinatarioService;

        private async void EnviarNotaCmd_ExecuteAsync(IClosable closable)
        {
            await EnviarNotaAsync(closable);
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
                Modelo1 = NFeCoreModelo.Modelo55;
                IsImpressaoBobina = false;
            }
            else
            {
                Modelo1 = NFeCoreModelo.Modelo65;
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
