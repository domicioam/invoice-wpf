using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DgSystems.NFe.ViewModels;
using EmissorNFe.Model;
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
using NFe.Core.NotasFiscais.Entities;
using NFe.WPF.NotaFiscal.Model;
using NFe.Core.Utils.PDF;
using NFe.Core.Messaging;
using DgSystems.NFe.ViewModels.Commands;

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

        private CancelarNotaViewModel _cancelarNotaViewModel;

        public bool IsDestinatarioEstrangeiro { get; set; }

        internal void VisualizarNotaFiscal(Core.NotasFiscais.NotaFiscal notaFiscal)
        {
            NotaFiscal = (NFCeModel)notaFiscal;
            NotaFiscal.Pagamentos = new ObservableCollection<PagamentoVO>();
            NotaFiscal.DestinatarioSelecionado = new DestinatarioModel();

            _notaFiscalBO = notaFiscal;

            //Preenche pagamentos
            if (_notaFiscalBO.Pagamentos != null)
            {

                foreach (var pagamento in _notaFiscalBO.Pagamentos)
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
            if (_notaFiscalBO.Destinatario != null)
            {
                DocumentoDestinatario = _notaFiscalBO.Destinatario.Documento.Numero;
                IsDestinatarioEstrangeiro = _notaFiscalBO.Destinatario.TipoDestinatario == TipoDestinatario.Estrangeiro;
            }

            NotaFiscal.Finalidade = _notaFiscalBO.Identificacao.FinalidadeConsumidor == FinalidadeConsumidor.ConsumidorFinal ? "Consumidor Final" : "Normal";
            NotaFiscal.NaturezaOperacao = _notaFiscalBO.Identificacao.NaturezaOperacao;
            NotaFiscal.Serie = _notaFiscalBO.Identificacao.Serie.ToString("D3");

            //Preenche produtos
            NotaFiscal.Produtos = new ObservableCollection<ProdutoVO>();

            foreach (var produto in _notaFiscalBO.Produtos)
            {
                var produtoVO = new ProdutoVO();
                produtoVO.QtdeProduto = produto.QtdeUnidadeComercial;
                produtoVO.Descricao = produto.Descricao;
                produtoVO.ValorUnitario = produto.ValorUnidadeComercial;
                produtoVO.Descontos = produto.Desconto;
                produtoVO.TotalLiquido = produto.ValorTotal;

                NotaFiscal.Produtos.Add(produtoVO);
            }

            var command = new OpenVisualizarNotaEnviadaWindowCommand(this);
            MessagingCenter.Send(this, nameof(OpenVisualizarNotaEnviadaWindowCommand), command);
        }

        public VisualizarNotaEnviadaViewModel(IDialogService dialogService, CancelarNotaViewModel cancelarNotaViewModel)
        {
            EmitirSegundaViaCmd = new RelayCommand(EmitirSegundaViaCmd_Execute, null);
            CancelarNotaCmd = new RelayCommand(CancelarNotaCmd_Execute, null);

            _dialogService = dialogService;
            _cancelarNotaViewModel = cancelarNotaViewModel;
        }

        private void CancelarNotaCmd_Execute()
        {
            _cancelarNotaViewModel.CancelarNotaFiscal(_notaFiscal);
        }

        private async void EmitirSegundaViaCmd_Execute()
        {
            var complemento = new RetornoNotaFiscal();
            _notaFiscalBO.DhAutorizacao = NotaFiscal.DataAutorizacao;
            _notaFiscalBO.ProtocoloAutorizacao = NotaFiscal.Protocolo;

            try
            {
                await GeradorPDF.GerarPdfNotaFiscal(_notaFiscalBO);
            }
            catch (Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Erro ao emitir segunda via, verifique sua impressora.", "Erro!", null, null);
            }
        }
    }
}
