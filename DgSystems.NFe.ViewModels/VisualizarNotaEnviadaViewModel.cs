using DgSystems.NFe.ViewModels;
using DgSystems.NFe.ViewModels.Commands;
using EmissorNFe.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core;
using NFe.Core.Domain;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;
using NFe.WPF.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace NFe.WPF.ViewModel
{
    public class VisualizarNotaEnviadaViewModel : ViewModelBaseValidation
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Core.Domain.NotaFiscal _notaFiscal;
        private string _documentoDestinatario;
        private Core.Domain.NotaFiscal _notaFiscalBO;

        public ICommand EmitirSegundaViaCmd { get; set; }
        public ICommand CancelarNotaCmd { get; set; }

        private IDialogService _dialogService;

        public object Destinatario { get; private set; }
        public string DataAutorizacao { get; private set; }
        public DateTime DataEmissao { get; private set; }
        public string Chave { get; private set; }
        public string Modelo { get; private set; }
        public string Numero { get; private set; }

        public Core.Domain.NotaFiscal NotaFiscal
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

        public ObservableCollection<PagamentoModel> Pagamentos { get; private set; }
        public DestinatarioModel DestinatarioSelecionado { get; private set; }

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
        public ObservableCollection<ProdutoModel> Produtos { get; private set; }
        public string Finalidade { get; private set; }
        public string NaturezaOperacao { get; private set; }
        public string Serie { get; private set; }

        internal void VisualizarNotaFiscal(Core.Domain.NotaFiscal notaFiscal)
        {
            NotaFiscal = notaFiscal;

            Destinatario = notaFiscal.Destinatario == null
                ? "CONSUMIDOR NÃO IDENTIFICADO"
                : notaFiscal.Destinatario.NomeRazao;

            DataAutorizacao = notaFiscal.DhAutorizacao;
            DataEmissao = notaFiscal.Identificacao.DataHoraEmissao;
            Chave = notaFiscal.Identificacao.Chave.ToString();
            Modelo = notaFiscal.Identificacao.Modelo == NFe.Core.Domain.Modelo.Modelo55 ? "NF-e" : "NFC-e";
            Numero = notaFiscal.Identificacao.Numero;

            Pagamentos = new ObservableCollection<PagamentoModel>();
            DestinatarioSelecionado = new DestinatarioModel();

            _notaFiscalBO = notaFiscal;

            //Preenche pagamentos
            if (_notaFiscalBO.Pagamentos != null)
            {

                foreach (var pagamento in _notaFiscalBO.Pagamentos)
                {
                    var pagamentoVO = new PagamentoModel();
                    pagamentoVO.FormaPagamento = pagamento.FormaPagamentoTexto;
                    pagamentoVO.ValorTotal = pagamento.Valor.ToString("N2", new CultureInfo("pt-BR"));

                    Pagamentos.Add(pagamentoVO);
                }
            }
            else
            {
                Pagamentos.Add(new PagamentoModel() { FormaPagamento = "N/A" });
            }

            //Preenche documento destinatário
            if (_notaFiscalBO.Destinatario != null)
            {
                DocumentoDestinatario = _notaFiscalBO.Destinatario.Documento.Numero;
                IsDestinatarioEstrangeiro = _notaFiscalBO.Destinatario.TipoDestinatario == TipoDestinatario.Estrangeiro;
            }

            Finalidade = _notaFiscalBO.Identificacao.FinalidadeConsumidor == FinalidadeConsumidor.ConsumidorFinal ? "Consumidor Final" : "Normal";
            NaturezaOperacao = _notaFiscalBO.Identificacao.NaturezaOperacao;
            Serie = _notaFiscalBO.Identificacao.Serie.ToString("D3");

            //Preenche produtos
            Produtos = new ObservableCollection<ProdutoModel>();

            foreach (var produto in _notaFiscalBO.Produtos)
            {
                var produtoVO = new ProdutoModel();
                produtoVO.QtdeProduto = produto.QtdeUnidadeComercial;
                produtoVO.Descricao = produto.Descricao;
                produtoVO.ValorUnitario = produto.ValorUnidadeComercial;
                produtoVO.Descontos = produto.Desconto;
                produtoVO.TotalLiquido = produto.ValorTotal;

                Produtos.Add(produtoVO);
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
            _cancelarNotaViewModel.CancelarNotaFiscal(_notaFiscal.Identificacao.Modelo, _notaFiscal.Identificacao.Chave, _notaFiscal.ProtocoloAutorizacao, _notaFiscal.Identificacao.Status, _notaFiscal.Identificacao.Numero, _notaFiscal.Identificacao.Serie.ToString());
        }

        private async void EmitirSegundaViaCmd_Execute()
        {
            var complemento = new RetornoNotaFiscal();
            _notaFiscalBO.DhAutorizacao = NotaFiscal.DhAutorizacao;
            _notaFiscalBO.ProtocoloAutorizacao = NotaFiscal.ProtocoloAutorizacao;

            try
            {
                var command = new ImprimirDanfeCommand(_notaFiscalBO);
                command.Execute();
                if (!command.IsExecuted)
                {
                    log.Error("Danfe não impresso.");
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Erro ao emitir segunda via, verifique sua impressora.", "Erro!", null, null);
            }
        }
    }
}
