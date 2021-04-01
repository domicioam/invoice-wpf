using EmissorNFe.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Transportadora;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Repository.Repositories;
using NFe.WPF.Commands;
using NFe.WPF.Events;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Input;
using NFeCoreModelo = NFe.Core.Domain.Modelo;

namespace DgSystems.NFe.ViewModels
{
    public class NFeViewModel : NotaFiscalModel
    {
        private const string DEFAULT_NATUREZA_OPERACAO = "Devolução";
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public NFeViewModel(IEnviarNotaAppService enviarNotaController, IDialogService dialogService, IProdutoRepository produtoRepository, IEstadoRepository estadoService, IEmissorService emissorService, IMunicipioRepository municipioService, ITransportadoraService transportadoraService, IDestinatarioService destinatarioService, INaturezaOperacaoRepository naturezaOperacaoService, IConfiguracaoRepository configuracaoService, DestinatarioViewModel destinatarioViewModel, ICertificadoService certificadoRepository)
        {
            Pagamento = new PagamentoModel();
            Produto = new ProdutoModel();
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
            _certificadoRepository = certificadoRepository;

            AdicionarProdutoCmd = new RelayCommand<object>(AdicionarProdutoCmd_Execute, null);
            GerarPagtoCmd = new RelayCommand<object>(GerarPagtoCmd_Execute, null);
            SalvarTransportadoraCmd = new RelayCommand<IClosable>(SalvarTransportadoraCmd_Execute, null);
            ExcluirTransportadoraCmd = new RelayCommand<TransportadoraModel>(ExcluirTransportadoraCmd_Execute, null);
            EnviarNotaCmd = new RelayCommand<IClosable>(EnviarNotaCmd_ExecuteAsync);
            LoadedCmd = new RelayCommand<string>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            ExcluirProdutoNotaCmd = new RelayCommand<ProdutoModel>(ExcluirProdutoNotaCmd_Execute, null);
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

            foreach (var produtoDb in produtosDb)
            {
                ProdutosCombo.Add(produtoDb);
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
            catch (Exception e)
            {
                Log.Error(e);
                _dialogService.ShowError("Não foi possível remover a transportadora.", "Erro!", null, null);
            }
        }

        private PagamentoModel _pagamento;
        private ProdutoModel _produto;
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
                NaturezaOperacao = value.Descricao;
                FiltrarProdutosCombo();
            }
        }

        private void FiltrarProdutosCombo()
        {
            ProdutosCombo.Clear();
            foreach (var produtoDB in _produtoRepository.GetProdutosByNaturezaOperacao(NaturezaOperacaoSelecionada.Descricao))
            {
                ProdutosCombo.Add(produtoDB);
            }
        }

        public ObservableCollection<DestinatarioModel> Destinatarios { get; set; }
        public ObservableCollection<TransportadoraModel> Transportadoras { get; set; }
        public ObservableCollection<NaturezaOperacaoModel> NaturezasOperacoes { get; set; }

        public List<string> IndicadoresPresenca { get; set; }
        public List<string> Finalidades { get; set; }
        public PagamentoModel Pagamento
        {
            get { return _pagamento; }
            set
            {
                _pagamento = value;
                RaisePropertyChanged(nameof(Pagamento));
            }
        }

        public Dictionary<string, string> FormasPagamento { get; set; }
        public List<string> EstadosUF { get; }
        public List<int> Parcelas { get; set; }

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

        private readonly IEnviarNotaAppService _enviarNotaController;
        private readonly IDialogService _dialogService;
        private readonly IEstadoRepository _estadoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IEmissorService _emissorService;
        private readonly IMunicipioRepository _municipioService;
        private readonly ITransportadoraService _transportadoraService;
        private readonly IDestinatarioService _destinatarioService;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly DestinatarioViewModel _destinatarioViewModel;
        private readonly ICertificadoService _certificadoRepository;

        private void SalvarTransportadoraCmd_Execute(IClosable closable)
        {
            TransportadoraParaSalvar.ValidateModel();

            if (TransportadoraParaSalvar.HasErrors)
                return;

            var transportadoraEntity = (TransportadoraEntity)TransportadoraParaSalvar;

            var transportadoraDal = new TransportadoraRepository();
            TransportadoraParaSalvar.Id = transportadoraDal.Salvar(transportadoraEntity);

            Transportadoras.Add(TransportadoraParaSalvar);
            TransportadoraSelecionada = TransportadoraParaSalvar;
            TransportadoraParaSalvar = new TransportadoraModel();
            closable.Close();
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

        private async void EnviarNotaCmd_ExecuteAsync(IClosable closable)
        {
            await EnviarNotaAsync(this, _modelo, closable);
        }

        private void AdicionarProdutoCmd_Execute(object obj)
        {
            Produto.ValidateModel();

            if (Produto.HasErrors)
                return;

            Produtos = Produtos ?? new ObservableCollection<ProdutoModel>();

            Produtos.Add(Produto);
            Pagamento.ValorParcela += Produto.TotalLiquido;
            ProdutosCombo.Remove(Produto.ProdutoSelecionado);

            RaisePropertyChanged(nameof(ProdutosCombo));
            RaisePropertyChanged("ProdutosGrid");
            Produto = new ProdutoModel();
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

            _destinatarioViewModel.DestinatarioParaSalvar = destSelecionado;

            var command = new AlterarDestinatarioCommand(_destinatarioViewModel);
            MessagingCenter.Send(this, nameof(AlterarDestinatarioCommand), command);

            destSelecionado.ValidateModel();
            if (!destSelecionado.HasErrors)
                return;

            Destinatarios.RemoveAt(destIndex);
            Destinatarios.Insert(destIndex, destSelecionado);
        }

        private void TransportadoraWindowLoadedCmd_Execute()
        {
            foreach (var estado in _estadoRepository.GetEstados())
            {
                Estados.Add(estado);
            }

            TransportadoraParaSalvar.Endereco.UF = _emissorService.GetEmissor().Endereco.UF;
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

        private void ExcluirProdutoNotaCmd_Execute(ProdutoModel produto)
        {
            Produtos.Remove(produto);
            ProdutosCombo.Add(produto.ProdutoSelecionado);
        }

        private void DestinatarioVM_DestinatarioSalvoEvent(DestinatarioModel destinatarioParaSalvar)
        {
            Destinatarios.Add(destinatarioParaSalvar);
            DestinatarioSelecionado = destinatarioParaSalvar;
        }

        private void ClosedCmd_Execute()
        {
            Produto = new ProdutoModel();
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

            var config = _configuracaoService.GetConfiguracao();

           Serie = config.SerieNFe;
           Numero = config.ProximoNumNFe;
           ModeloNota = "NF-e";

           DataEmissao = DateTime.Now;
           HoraEmissao = DateTime.Now;
           DataSaida = DateTime.Now;
           HoraSaida = DateTime.Now;
           IndicadorPresenca = PresencaComprador.Presencial;
           Finalidade = "Normal";

            if (Destinatarios.Count == 0)
            {
                foreach (var destDB in _destinatarioService.GetAll())
                {
                    Destinatarios.Add((DestinatarioModel)destDB);
                }
            }

            if (Transportadoras.Count == 0)
            {
                foreach (var transpDB in _transportadoraService.GetAll())
                {
                    Transportadoras.Add((TransportadoraModel)transpDB);
                }
            }

            if (NaturezasOperacoes.Count <= 0)
            {
                foreach (var naturezaDB in _naturezaOperacaoRepository.GetAll())
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

        public async Task EnviarNotaAsync(NotaFiscalModel NotaFiscal, Modelo _modelo, IClosable closable)
        {
            if (!NotaFiscal.NaturezaOperacao.Equals("Venda"))
            {
                NotaFiscal.Pagamentos = new ObservableCollection<PagamentoModel>() { new PagamentoModel() { FormaPagamento = "Sem Pagamento" } };
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
                X509Certificate2 certificado = _certificadoRepository.GetX509Certificate2();
                var emissor = _emissorService.GetEmissor();
                var notaFiscal = await _enviarNotaController.EnviarNotaAsync(NotaFiscal, _modelo, emissor, certificado, _dialogService);
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
                Log.Error(e);
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + e.InnerException.Message, "Erro", "Ok", null);
            }
            catch (Exception e)
            {
                Log.Error(e);
                await _dialogService.ShowError("Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + e.InnerException.Message, "Erro", "Ok", null);
            }
            finally
            {
                IsBusy = false;
                closable.Close();
            }
        }

        private TransportadoraModel _transportadoraSelecionada;
        private string _placaVeiculo;
        private string _ufVeiculo;

        [Required]
        public TransportadoraModel TransportadoraSelecionada
        {
            get { return _transportadoraSelecionada; }
            set
            {
                SetProperty(ref _transportadoraSelecionada, value);
            }
        }

        [Required]
        public string PlacaVeiculo
        {
            get { return _placaVeiculo; }
            set { SetProperty(ref _placaVeiculo, value); }
        }

        [Required]
        public string UfVeiculo
        {
            get { return _ufVeiculo; }
            set { SetProperty(ref _ufVeiculo, value); }
        }
    }
}
