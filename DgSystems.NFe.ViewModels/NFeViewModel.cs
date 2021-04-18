using EmissorNFe.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Certificado;
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
using System.Threading.Tasks;
using System.Windows.Input;
using NFeCoreModelo = NFe.Core.Domain.Modelo;

namespace DgSystems.NFe.ViewModels
{
    public class NFeViewModel : NotaFiscalModel
    {
        private const string DEFAULT_NATUREZA_OPERACAO = "Devolução";

        public NFeViewModel(IEnviarNotaAppService enviarNotaAppService, IDialogService dialogService, IProdutoRepository produtoRepository, IEstadoRepository estadoService, IEmitenteRepository emissorService, IMunicipioRepository municipioService, ITransportadoraRepository transportadoraService, IDestinatarioRepository destinatarioService, INaturezaOperacaoRepository naturezaOperacaoService, IConfiguracaoRepository configuracaoService, DestinatarioViewModel destinatarioViewModel, ICertificadoService certificadoRepository) : base(enviarNotaAppService, dialogService, emissorService, certificadoRepository)
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
            EnviarNotaCmd = new RelayCommand<IClosable>(EnviarNotaCmd_ExecuteAsync);
            LoadedCmd = new RelayCommand<string>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            ExcluirProdutoNotaCmd = new RelayCommand<ProdutoModel>(ExcluirProdutoNotaCmd_Execute, null);
            UfSelecionadoCmd = new RelayCommand(UfSelecionadoCmd_Execute, null);
            TransportadoraWindowLoadedCmd = new RelayCommand(TransportadoraWindowLoadedCmd_Execute, null);
            DestinatarioChangedCmd = new RelayCommand<DestinatarioModel>(DestinatarioChangedCmd_Execute, null);


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

            EstadosUF = _estadoRepository.GetEstados().ConvertAll(e => e.Uf);
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

        private NaturezaOperacaoModel _naturezaOperacaoParaSalvar;

        public ObservableCollection<EstadoEntity> Estados { get; set; }
        public ObservableCollection<MunicipioEntity> Municipios { get; set; }
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

        public List<string> IndicadoresPresenca => new List<string>()
        {
            "Não se Aplica",
            "Presencial",
            "Não Presencial, Internet",
            "Não Presencial, Teleatendimento",
            "Entrega a Domicílio",
            "Não Presencial, Outros"
        };

        public List<string> EstadosUF { get; }

        public ObservableCollection<ProdutoEntity> ProdutosCombo { get; set; }

        #region Commands
        public ICommand UfSelecionadoCmd { get; set; }
        public ICommand TransportadoraWindowLoadedCmd { get; set; }
        public ICommand DestinatarioChangedCmd { get; set; }
        public ICommand ExcluirTransportadoraCmd { get; set; }
        #endregion Commands

        private readonly IEstadoRepository _estadoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMunicipioRepository _municipioService;
        private readonly ITransportadoraRepository _transportadoraService;
        private readonly IDestinatarioRepository _destinatarioService;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly DestinatarioViewModel _destinatarioViewModel;

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
            await EnviarNotaAsync(closable);
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
                Modelo1 = NFeCoreModelo.Modelo55;
                IsImpressaoBobina = false;
            }
            else
            {
                Modelo1 = NFeCoreModelo.Modelo65;
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

            if (NaturezasOperacoes.Count == 0)
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

        protected override async Task EnviarNotaAsync(IClosable closable)
        {
            if (!NaturezaOperacao.Equals("Venda"))
            {
                Pagamentos = new ObservableCollection<PagamentoModel>() { new PagamentoModel() { FormaPagamento = "Sem Pagamento" } };
            }

            await base.EnviarNotaAsync(closable);
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
