using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Model;
using EmissorNFe.View.NotaFiscal;
using EmissorNFe.VO;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils.Xml;
using NFe.WPF.ViewModel.Base;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;
using NFe.Core.Interfaces;

namespace NFe.WPF.ViewModel
{
    public class VisualizarNotaEnviadaViewModel : ViewModelBaseValidation
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private NFCeModel _notaFiscal;
        private string _documentoDestinatario;
        private Core.NotasFiscais.NotaFiscal _notaFiscalBO;

        public ICommand EmitirSegundaViaCmd { get; set; }
        public ICommand CancelarNotaCmd { get; set; }

        private IDialogService _dialogService;

        public NFCeModel NotaFiscal
        {
            get
            {
                return _notaFiscal;
            }
            set
            {
                SetProperty(ref _notaFiscal, value);
            }
        }

        public string DocumentoDestinatario
        {
            get
            {
                return _documentoDestinatario;
            }
            private set
            {
                SetProperty(ref _documentoDestinatario, value);
            }
        }

        private bool _isDestinatarioEstrangeiro;
        private IConfiguracaoService _configuracaoService;
        private IEnviaNotaFiscalFacade _enviaNotaFiscalService;
        private CancelarNotaViewModel _cancelarNotaViewModel;
        private IEmissorService _emissorService;
        private INotaInutilizadaService _notaInutilizadaService;
        private INotaFiscalRepository _notaFiscalRepository;

        public bool IsDestinatarioEstrangeiro
        {
            get { return _isDestinatarioEstrangeiro; }
            set { _isDestinatarioEstrangeiro = value; }
        }

        internal async void VisualizarNotaFiscal(NFCeModel notaFiscal)
        {
            NotaFiscal = notaFiscal; //falta preencher pagamentos
            NotaFiscal.Pagamentos = new ObservableCollection<PagamentoVO>();
            NotaFiscal.DestinatarioSelecionado = new DestinatarioModel();
            string xml = await GetNotaXmlAsync();

            var notaFiscalBO = _notaFiscalRepository.GetNotaFiscalFromNfeProcXml(xml);
            _notaFiscalBO = notaFiscalBO;

            //Preenche pagamentos
            if (notaFiscalBO.Pagamentos != null)
            {

                foreach (var pagamento in notaFiscalBO.Pagamentos)
                {
                    var pagamentoVO = new PagamentoVO();
                    pagamentoVO.FormaPagamento = pagamento.FormaPagamentoTexto;
                    pagamentoVO.ValorTotal = pagamento.Valor.ToString("N2", new CultureInfo("pt-BR"));

                    NotaFiscal.Pagamentos.Add(pagamentoVO);
                }
            }
            else
            {
                NotaFiscal.Pagamentos.Add(new PagamentoVO() { FormaPagamento = "N/A" });
            }

            //Preenche documento destinatário
            if (notaFiscalBO.Destinatario != null)
            {
                DocumentoDestinatario = notaFiscalBO.Destinatario.Documento;
                IsDestinatarioEstrangeiro = notaFiscalBO.Destinatario.TipoDestinatario == TipoDestinatario.Estrangeiro;
            }

            NotaFiscal.Finalidade = notaFiscalBO.Identificacao.FinalidadeConsumidor == FinalidadeConsumidor.ConsumidorFinal ? "Consumidor Final" : "Normal";
            NotaFiscal.NaturezaOperacao = notaFiscalBO.Identificacao.NaturezaOperacao;
            NotaFiscal.Serie = notaFiscalBO.Identificacao.Serie.ToString("D3");

            //Preenche produtos
            NotaFiscal.Produtos = new ObservableCollection<ProdutoVO>();

            foreach (var produto in notaFiscalBO.Produtos)
            {
                var produtoVO = new ProdutoVO();
                produtoVO.QtdeProduto = produto.QtdeUnidadeComercial;
                produtoVO.Descricao = produto.Descricao;
                produtoVO.ValorUnitario = produto.ValorUnidadeComercial;
                produtoVO.Descontos = produto.ValorDesconto;
                produtoVO.TotalLiquido = produto.ValorTotal;

                NotaFiscal.Produtos.Add(produtoVO);
            }

            var app = Application.Current;
            var mainWindow = app.MainWindow;
            var window = new VisualizarNotaEnviadaWindow() { Owner = mainWindow };
            window.ShowDialog();
        }

        private async Task<string> GetNotaXmlAsync()
        {
            var config = await _configuracaoService.GetConfiguracaoAsync();
            var ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;

            var notaDb = _notaFiscalRepository.GetNotaFiscalByChave(NotaFiscal.Chave);
            string xml = await notaDb.LoadXmlAsync();
            return xml;
        }

        public VisualizarNotaEnviadaViewModel(IDialogService dialogService, IEnviaNotaFiscalFacade enviaNotaFiscalService, IConfiguracaoService configuracaoService, CancelarNotaViewModel cancelarNotaViewModel, IEmissorService emissorService, INotaInutilizadaService notaInutilizadaService, INotaFiscalRepository notaFiscalRepository)
        {
            EmitirSegundaViaCmd = new RelayCommand(EmitirSegundaViaCmd_Execute, null);
            CancelarNotaCmd = new RelayCommand(CancelarNotaCmd_Execute, null);

            _dialogService = dialogService;
            _enviaNotaFiscalService = enviaNotaFiscalService;
            _configuracaoService = configuracaoService;
            _cancelarNotaViewModel = cancelarNotaViewModel;
            _emissorService = emissorService;
            _notaInutilizadaService = notaInutilizadaService;
            _notaFiscalRepository = notaFiscalRepository;
        }

        private void CancelarNotaCmd_Execute()
        {
            var notaFiscal = (NFCeModel)_notaFiscalRepository.GetNotaFiscalByChave(NotaFiscal.Chave);
            _cancelarNotaViewModel.CancelarNotaFiscal(notaFiscal);
        }

        private async void EmitirSegundaViaCmd_Execute()
        {
            var complemento = new RetornoNotaFiscal();
            _notaFiscalBO.DhAutorizacao = NotaFiscal.DataAutorizacao;
            _notaFiscalBO.ProtocoloAutorizacao = NotaFiscal.Protocolo;

            string xml = await GetNotaXmlAsync();
            _notaFiscalBO.QrCodeUrl = xml;

            try
            {
                await Reports.PDF.GeradorPDF.GerarPdfNotaFiscal(_notaFiscalBO);
            }
            catch (Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Erro ao emitir segunda via, verifique sua impressora.", "Erro!", null, null);
            }
        }
    }
}
