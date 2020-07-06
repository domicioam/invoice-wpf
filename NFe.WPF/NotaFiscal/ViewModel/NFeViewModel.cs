using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EmissorNFe.Model;
using EmissorNFe.VO;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Transportadora;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais;
using NFe.Repository.Repositories;
using NFe.WPF.Events;
using NFe.WPF.Model;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Base;
using NFe.WPF.ViewModel.Services;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public class NFeViewModel : ViewModelBaseValidation
    {
        private const string DEFAULT_NATUREZA_OPERACAO = "Remessa de vasilhames";
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public NFeViewModel(IEnviarNota enviarNotaController, IDialogService dialogService, IProdutoRepository produtoRepository, IEstadoRepository estadoService, IEmissorService emissorService, IMunicipioRepository municipioService, ITransportadoraService transportadoraService, IDestinatarioService destinatarioService, INaturezaOperacaoRepository naturezaOperacaoService, IConfiguracaoService configuracaoService, DestinatarioViewModel destinatarioViewModel)
        {
            Pagamento = new PagamentoVO();
            Produto = new ProdutoVO();
            var produtosDb = produtoRepository.GetAll();
            DestinatarioParaSalvar = new DestinatarioModel();
            TransportadoraParaSalvar = new TransportadoraModel();
            Destinatarios = new ObservableCollection<DestinatarioModel>();
            Transportadoras = new ObservableCollection<TransportadoraModel>();
            NaturezasOperacoes = new ObservableCollection<NaturezaOperacaoModel>();
            ProdutosCombo = new ObservableCollection<ProdutoEntity>();

            _estadoRepository = estadoService;
            _produtoRepository = produtoRepository;
            _emissorService = emissorService;
            _municipioService = municipioService;
            _transportadoraService = transportadoraService;
            _destinatarioService = destinatarioService;
            _naturezaOperacaoRepository = naturezaOperacaoService;
            _configuracaoService = configuracaoService;
            _destinatarioViewModel = destinatarioViewModel;

            AdicionarProdutoCmd = new RelayCommand<object>(AdicionarProdutoCmd_Execute, null);
            GerarPagtoCmd = new RelayCommand<object>(GerarPagtoCmd_Execute, null);
            SalvarTransportadoraCmd = new RelayCommand<IClosable>(SalvarTransportadoraCmd_Execute, null);
            ExcluirTransportadoraCmd = new RelayCommand<TransportadoraModel>(ExcluirTransportadoraCmd_Execute, null);
            EnviarNotaCmd = new RelayCommand<IClosable>(EnviarNotaCmd_Execute);
            LoadedCmd = new RelayCommand<string>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            ExcluirProdutoNotaCmd = new RelayCommand<ProdutoVO>(ExcluirProdutoNotaCmd_Execute, null);
            UfSelecionadoCmd = new RelayCommand(UfSelecionadoCmd_Execute, null);
            TransportadoraWindowLoadedCmd = new RelayCommand(TransportadoraWindowLoadedCmd_Execute, null);
            DestinatarioChangedCmd = new RelayCommand<DestinatarioModel>(DestinatarioChangedCmd_Execute, null);

            _enviarNotaController = enviarNotaController;
            _dialogService = dialogService;

            Estados = new ObservableCollection<EstadoEntity>();
            Municipios = new ObservableCollection<MunicipioEntity>();

            MessagingCenter.Subscribe<DestinatarioViewModel, DestinatarioSalvoEvent>(this, nameof(DestinatarioSalvoEvent), (s, e) =>
            {
                DestinatarioVM_DestinatarioSalvoEvent(e.Destinatario);
            });

            foreach (var produtoDB in produtosDb)
            {
                ProdutosCombo.Add((ProdutoEntity)produtoDB);
            }

            IndicadoresPresenca = new List<string>()
            {
                "Não se Aplica",
                "Presencial",
                "Não Presencial, Internet",
                "Não Presencial, Teleatendimento",
                "Entrega a Domicílio",
                "Não Presencial, Outros"
            };

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

            EstadosUF = _estadoRepository.GetEstados().Select(e => e.Uf).ToList();

            Parcelas = new List<int>()
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
            };
        }

        private void ExcluirTransportadoraCmd_Execute(TransportadoraModel transportadora)
        {
            try
            {
                var transportadoraRepository = new TransportadoraRepository();
                transportadoraRepository.Excluir(transportadora.Id);
                Transportadoras.Remove(transportadora);
                _dialogService.ShowMessageBox("Transportadora removida com sucesso!", "Sucesso!");
            }
            catch(Exception e)
            {
                log.Error(e);
                _dialogService.ShowError("Não foi possível remover a transportadora.", "Erro!", null, null);
            }
        }

        private NFeModel _notaFiscal;

        public NFeModel NotaFiscal
        {
            get { return _notaFiscal; }
            set { SetProperty(ref _notaFiscal, value); }
        }

        private PagamentoVO _pagamento;
        private ProdutoVO _produto;
        private Modelo _modelo;
        private NaturezaOperacaoModel _naturezaOperacaoParaSalvar;

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
        public ObservableCollection<EstadoEntity> Estados { get; set; }
        public ObservableCollection<MunicipioEntity> Municipios { get; set; }
        public DestinatarioModel DestinatarioParaSalvar { get; set; }
        public TransportadoraModel TransportadoraParaSalvar { get; set; }
        public NaturezaOperacaoModel NaturezaOperacaoSelecionada
        {
            get
            {
                return _naturezaOperacaoParaSalvar;
            }
            set
            {
                SetProperty(ref _naturezaOperacaoParaSalvar, value);
                NotaFiscal.NaturezaOperacao = value.Descricao;
                FiltrarProdutosCombo();
            }
        }

        private void FiltrarProdutosCombo()
        {
            ProdutosCombo.Clear();
            var produtosDb = _produtoRepository.GetProdutosByNaturezaOperacao(NaturezaOperacaoSelecionada.Descricao);
            foreach (var produtoDB in produtosDb)
            {
                ProdutosCombo.Add(produtoDB);
            }
        }

        public ObservableCollection<DestinatarioModel> Destinatarios { get; set; }
        public ObservableCollection<TransportadoraModel> Transportadoras { get; set; }
        public ObservableCollection<NaturezaOperacaoModel> NaturezasOperacoes { get; set; }

        public List<string> IndicadoresPresenca { get; set; }
        public List<string> Finalidades { get; set; }
        public PagamentoVO Pagamento
        {
            get { return _pagamento; }
            set
            {
                _pagamento = value;
                RaisePropertyChanged("Pagamento");
            }
        }

        public Dictionary<string, string> FormasPagamento { get; set; }
        public List<string> EstadosUF { get; private set; }
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

        #region Commands

        public ICommand SalvarTransportadoraCmd { get; set; }
        public ICommand AdicionarProdutoCmd { get; set; }
        public ICommand GerarPagtoCmd { get; set; }
        public ICommand EnviarNotaCmd { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand ClosedCmd { get; set; }
        public ICommand ExcluirProdutoNotaCmd { get; set; }
        public ICommand ExcluirPagamentoCmd { get; set; }
        public ICommand UfSelecionadoCmd { get; set; }
        public ICommand TransportadoraWindowLoadedCmd { get; set; }
        public ICommand DestinatarioChangedCmd { get; set; }
        public ICommand ExcluirTransportadoraCmd { get; set; }
        #endregion Commands

        private IEnviarNota _enviarNotaController;
        private IDialogService _dialogService;
        private IEstadoRepository _estadoRepository;
        private IProdutoRepository _produtoRepository;
        private IEmissorService _emissorService;
        private IMunicipioRepository _municipioService;
        private ITransportadoraService _transportadoraService;
        private IDestinatarioService _destinatarioService;
        private INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private IConfiguracaoService _configuracaoService;
        private DestinatarioViewModel _destinatarioViewModel;


        private void SalvarTransportadoraCmd_Execute(IClosable closable)
        {
            TransportadoraParaSalvar.ValidateModel();

            if (TransportadoraParaSalvar.HasErrors) 
                return;

            var transportadoraEntity = (TransportadoraEntity)TransportadoraParaSalvar;

            var transportadoraDal = new TransportadoraRepository();
            var id = transportadoraDal.Salvar(transportadoraEntity);
            TransportadoraParaSalvar.Id = id;

            Transportadoras.Add(TransportadoraParaSalvar);
            NotaFiscal.TransportadoraSelecionada = TransportadoraParaSalvar;
            TransportadoraParaSalvar = new TransportadoraModel();
            closable.Close();
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

        private async void EnviarNotaCmd_Execute(IClosable closable)
        {
            await EnviarNota(NotaFiscal, _modelo, closable);
        }

        private void AdicionarProdutoCmd_Execute(object obj)
        {
            Produto.ValidateModel();

            if (Produto.HasErrors) 
                return;

            NotaFiscal.Produtos = NotaFiscal.Produtos ?? new ObservableCollection<ProdutoVO>();

            NotaFiscal.Produtos.Add(Produto);
            Pagamento.ValorParcela += Produto.TotalLiquido;
            ProdutosCombo.Remove(Produto.ProdutoSelecionado);

            RaisePropertyChanged("ProdutosCombo");
            RaisePropertyChanged("ProdutosGrid");
            Produto = new ProdutoVO();
        }

        private void DestinatarioChangedCmd_Execute(DestinatarioModel destSelecionado)
        {
            if (destSelecionado == null)
                return;

            destSelecionado.IsNFe = true;
            destSelecionado.ValidateModel();
            int destIndex = Destinatarios.IndexOf(destSelecionado);

            if (destIndex == -1)
                return;

            if (!destSelecionado.HasErrors) 
                return;

            _destinatarioViewModel.AlterarDestinatario(destSelecionado);

            destSelecionado.ValidateModel();
            if (!destSelecionado.HasErrors) 
                return;

            Destinatarios.RemoveAt(destIndex);
            Destinatarios.Insert(destIndex, destSelecionado);
        }

        private void TransportadoraWindowLoadedCmd_Execute()
        {
            var estados = _estadoRepository.GetEstados();

            foreach (var estado in estados)
            {
                Estados.Add(estado);
            }

            var emitenteUf = _emissorService.GetEmissor().Endereco.UF;
            TransportadoraParaSalvar.Endereco.UF = emitenteUf;
            UfSelecionadoCmd_Execute();
        }

        private void UfSelecionadoCmd_Execute()
        {
            var municipios = _municipioService.GetMunicipioByUf(TransportadoraParaSalvar.Endereco.UF);

            Municipios.Clear();

            foreach (var municipio in municipios)
            {
                Municipios.Add(municipio);
            }
        }

        private void ExcluirProdutoNotaCmd_Execute(ProdutoVO produto)
        {
            NotaFiscal.Produtos.Remove(produto);
            ProdutosCombo.Add(produto.ProdutoSelecionado);
        }

        private void DestinatarioVM_DestinatarioSalvoEvent(DestinatarioModel destinatarioParaSalvar)
        {
            if (NotaFiscal == null) 
                return;

            Destinatarios.Add(destinatarioParaSalvar);
            NotaFiscal.DestinatarioSelecionado = destinatarioParaSalvar;
        }

        private void ClosedCmd_Execute()
        {
            NotaFiscal = null;
            Produto = new ProdutoVO();
        }

        private void LoadedCmd_Execute(string modelo)
        {
            NotaFiscal = new NFeModel();

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
            Pagamento = new PagamentoVO {FormaPagamento = "Dinheiro"};

            var config = _configuracaoService.GetConfiguracao();

            NotaFiscal.Serie = config.SerieNFe;
            NotaFiscal.Numero = config.ProximoNumNFe;
            NotaFiscal.ModeloNota = "NF-e";

            NotaFiscal.DataEmissao = DateTime.Now;
            NotaFiscal.HoraEmissao = DateTime.Now;
            NotaFiscal.DataSaida = DateTime.Now;
            NotaFiscal.HoraSaida = DateTime.Now;
            NotaFiscal.IndicadorPresenca = PresencaComprador.Presencial;
            NotaFiscal.Finalidade = "Normal";

            if (Destinatarios.Count <= 0)
            {
                var destinatarios = _destinatarioService.GetAll();

                foreach (var destDB in destinatarios)
                {
                    Destinatarios.Add((DestinatarioModel)destDB);
                }
            }

            if (Transportadoras.Count <= 0)
            {
                var transportadoras = _transportadoraService.GetAll();

                foreach (var transpDB in transportadoras)
                {
                    Transportadoras.Add((TransportadoraModel)transpDB);
                }
            }

            if (NaturezasOperacoes.Count <= 0)
            {
                var naturezasDB = _naturezaOperacaoRepository.GetAll();

                foreach (var naturezaDB in naturezasDB)
                {
                    var natModel = new NaturezaOperacaoModel() { Id = naturezaDB.Id, Descricao = naturezaDB.Descricao };
                    NaturezasOperacoes.Add(natModel);

                    if (natModel.Descricao.Equals(DEFAULT_NATUREZA_OPERACAO))
                    {
                        NaturezaOperacaoSelecionada = natModel;
                    }
                }
            }
            else
            {
                NaturezaOperacaoSelecionada = NaturezasOperacoes.FirstOrDefault(n => n.Descricao.Equals(DEFAULT_NATUREZA_OPERACAO));
            }
        }

        public async Task EnviarNota(NotaFiscalModel NotaFiscal, Modelo _modelo, IClosable closable)
        {
            if (!NotaFiscal.NaturezaOperacao.Equals("Venda"))
            {
                NotaFiscal.Pagamentos = new ObservableCollection<PagamentoVO>() { new PagamentoVO() { FormaPagamento = "Sem Pagamento" } };
            }

            NotaFiscal.ValidateModel();

            if (NotaFiscal.HasErrors)
            {
                return;
            }

            BusyContent = "Enviando...";
            IsBusy = true;

            try
            {
               var notaFiscal = await _enviarNotaController.EnviarNota(NotaFiscal, _modelo);
                IsBusy = false;
                bool result = await _dialogService.ShowMessage("Nota enviada com sucesso! Deseja imprimi-la?", "Emissão NFe", "Sim", "Não", null);
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
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + e.InnerException.Message, "Erro", "Ok", null);
            }
            catch (Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + e.InnerException.Message, "Erro", "Ok", null);
            }
            finally
            {
                IsBusy = false;
                closable.Close();
            }
        }
    }
}
