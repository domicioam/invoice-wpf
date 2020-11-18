using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmissorNFe.Model;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.NotasFiscais.ValueObjects;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Acentuacao;
using NFe.Core.Utils.PDF;
using NFe.WPF.Events;
using NFe.WPF.Model;
using NFe.WPF.NotaFiscal.Model;
using Destinatario = NFe.Core.NotasFiscais.Entities.Destinatario;
using Emissor = NFe.Core.NotasFiscais.Emissor;
using Pagamento = NFe.Core.NotasFiscais.Pagamento;
using Produto = NFe.Core.NotasFiscais.Entities.Produto;
using DgSystems.NFe.Extensions;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public class EnviarNotaController : IEnviarNota
    {
        private readonly IDialogService _dialogService;
        private readonly IEnviaNotaFiscalFacade _enviaNotaFiscalService;
        private readonly IConfiguracaoService _configuracaoService;
        private readonly IEmissorService _emissorService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly SefazSettings _sefazSettings;

        public EnviarNotaController(IDialogService dialogService, IEnviaNotaFiscalFacade enviaNotaFiscalService,
            IConfiguracaoService configuracaoService, IEmissorService emissorService, IProdutoRepository produtoRepository, SefazSettings sefazSettings)
        {
            _dialogService = dialogService;
            _enviaNotaFiscalService = enviaNotaFiscalService;
            _configuracaoService = configuracaoService;
            _emissorService = emissorService;
            _produtoRepository = produtoRepository;
            _sefazSettings = sefazSettings;
        }

        public async Task<Core.NotasFiscais.NotaFiscal> EnviarNota(NotaFiscalModel notaFiscalModel, Modelo modelo)
        {
            notaFiscalModel.ValidateModel();

            if (notaFiscalModel.HasErrors)
                throw new NotaFiscalModelHasErrorsException("Nota fiscal contém erros de validação não resolvidos.");

            double valorTotalProdutos = notaFiscalModel.Produtos.Sum(c => c.QtdeProduto * c.ValorUnitario);
            double valorTotalPagamentos = notaFiscalModel.Pagamentos.Sum(p => p.QtdeParcelas * p.ValorParcela);
            bool isNotaComPagamento = notaFiscalModel.Pagamentos[0].FormaPagamento != "Sem Pagamento";

            if (isNotaComPagamento && valorTotalProdutos != valorTotalPagamentos)
            {
                await _dialogService.ShowError("Valor total da nota não corresponde ao valor de pagamento.",
                    "Erro!", "Ok", null);
                throw new ArgumentException("Valor total da nota não corresponde ao valor de pagamento.");
            }

            var config = _configuracaoService.GetConfiguracao();
            Core.NotasFiscais.NotaFiscal notaFiscal = null;

            await Task.Run(() =>
           {
               const TipoEmissao tipoEmissao = TipoEmissao.Normal;
               var destinatario = CreateDestinatario(notaFiscalModel, _sefazSettings.Ambiente, modelo);
               var documentoDanfe = destinatario != null ? destinatario.Documento.GetDocumentoDanfe(destinatario.TipoDestinatario) : "CPF";
               var emitente = _emissorService.GetEmissor();
               var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);

               var identificacao = CreateIdentificacaoNFe(notaFiscalModel, codigoUF, DateTime.Now, emitente, modelo,
                   Convert.ToInt32(notaFiscalModel.Serie), notaFiscalModel.Numero, tipoEmissao, _sefazSettings.Ambiente, documentoDanfe);
               var produtos = GetProdutos(notaFiscalModel, config);
               var pagamentos = GetPagamentos(notaFiscalModel);
               var totalNFe = GetTotalNFe(notaFiscalModel);
               var infoAdicional = new InfoAdicional(produtos);
               var transporte = GetTransporte(notaFiscalModel, modelo);

               notaFiscal = new Core.NotasFiscais.NotaFiscal(emitente, destinatario, identificacao, transporte,
                   totalNFe, infoAdicional, produtos, pagamentos);

               var cscId = config.CscId;
               var csc = config.Csc;
               _enviaNotaFiscalService.EnviarNotaFiscal(notaFiscal, cscId, csc);
           });

            var theEvent = new NotaFiscalEnviadaEvent() { NotaFiscal = notaFiscal };
            MessagingCenter.Send(this, nameof(NotaFiscalEnviadaEvent), theEvent);

            return notaFiscal;
        }

        public async Task ImprimirNotaFiscal(Core.NotasFiscais.NotaFiscal notaFiscal)
        {
            await GeradorPDF.GerarPdfNotaFiscal(notaFiscal);
        }

        private static Destinatario CreateDestinatario(NotaFiscalModel notaFiscal, Ambiente ambiente, Modelo _modelo)
        {
            if (notaFiscal.DestinatarioSelecionado.Documento == null &&
                string.IsNullOrEmpty(notaFiscal.Documento)) return null;

            string documento, nomeRazao, inscricaoEstadual = null;
            DestinatarioModel destinatarioSelecionado = null;
            Endereco endereco = null;

            if (notaFiscal.DestinatarioSelecionado.Documento != null)
            {
                destinatarioSelecionado = notaFiscal.DestinatarioSelecionado;

                if (notaFiscal.DestinatarioSelecionado.Endereco.Logradouro != null)
                {
                    var enderecoModel = notaFiscal.DestinatarioSelecionado.Endereco;
                    endereco = new Endereco(enderecoModel.Logradouro, enderecoModel.Numero, enderecoModel.Bairro,
                        enderecoModel.Municipio, enderecoModel.CEP, enderecoModel.UF);
                }

                inscricaoEstadual = notaFiscal.DestinatarioSelecionado?.InscricaoEstadual;
                documento = notaFiscal.DestinatarioSelecionado.Documento;
                nomeRazao = notaFiscal.DestinatarioSelecionado.NomeRazao;
            }
            else
            {
                documento = notaFiscal.Documento;
                nomeRazao = "CONSUMIDOR NÃO IDENTIFICADO";
            }

            TipoDestinatario tipoDestinatario;

            if (notaFiscal.IsEstrangeiro)
                tipoDestinatario = TipoDestinatario.Estrangeiro;
            else if (documento.Length == 11)
                tipoDestinatario = TipoDestinatario.PessoaFisica;
            else
                tipoDestinatario = TipoDestinatario.PessoaJuridica;


            var destinatario = new Destinatario(ambiente, _modelo, destinatarioSelecionado?.Telefone,
                destinatarioSelecionado?.Email, endereco, tipoDestinatario, inscricaoEstadual, documento: new Documento(documento),
                nomeRazao: nomeRazao);

            return destinatario;
        }

        private static IdentificacaoNFe CreateIdentificacaoNFe(NotaFiscalModel notaFiscal, CodigoUfIbge codigoUf, DateTime now,
            Emissor emitente, Modelo modeloNota,
            int serie, string numeroNFe, TipoEmissao tipoEmissao, Ambiente ambiente, string documentoDanfe)
        {
            var finalidadeEmissao = (FinalidadeEmissao)Enum.Parse(typeof(FinalidadeEmissao), Acentuacao.RemoverAcentuacao(notaFiscal.Finalidade));

            var identificacao = new IdentificacaoNFe(codigoUf, now, emitente.CNPJ, modeloNota, serie, numeroNFe,
                tipoEmissao, ambiente, emitente,
                notaFiscal.NaturezaOperacao, finalidadeEmissao, notaFiscal.IsImpressaoBobina,
                notaFiscal.IndicadorPresenca, documentoDanfe);

            return identificacao;
        }

        private static TotalNFe GetTotalNFe(NotaFiscalModel notaFiscal)
        {

            double sumValorTotal = 0;
            double sumValorTotalFrete = 0;
            double sumValorTotalSeguro = 0;
            double sumValorTotalDesconto = 0;

            foreach (var produto in notaFiscal.Produtos)
            {
                sumValorTotal += produto.TotalLiquido;
                sumValorTotalFrete += produto.Frete;
                sumValorTotalSeguro += produto.Seguro;
                sumValorTotalDesconto += produto.Descontos;
            }

            var totalNFe = new TotalNFe { IcmsTotal = new IcmsTotal() };
            var icmsTotal = totalNFe.IcmsTotal;
            icmsTotal.BaseCalculo = 0.00;
            icmsTotal.ValorTotalIcms = 0.00;
            icmsTotal.ValorTotalDesonerado = 0.00;
            icmsTotal.BaseCalculoST = 0.00;
            icmsTotal.ValorTotalST = 0.00;
            icmsTotal.ValorTotalProdutos = sumValorTotal;
            icmsTotal.ValorTotalFrete = sumValorTotalFrete;
            icmsTotal.ValorTotalSeguro = sumValorTotalSeguro;
            icmsTotal.ValorTotalDesconto = sumValorTotalDesconto;
            icmsTotal.ValorTotalII = 0.00;
            icmsTotal.ValorTotalIpi = 0.00;
            icmsTotal.ValorTotalPis = 0.00;
            icmsTotal.ValorTotalCofins = 0.00;
            icmsTotal.ValorDespesasAcessorias = 0.00;
            icmsTotal.ValorTotalNFe = sumValorTotal;

            throw new NotImplementedException();
            //return totalNFe;
        }

        private static List<Pagamento> GetPagamentos(NotaFiscalModel notaFiscal)
        {
            var pagamentosNf = new List<Pagamento>();

            foreach (var pagamento in notaFiscal.Pagamentos)
            {
                FormaPagamento formaPagamento;

                switch (pagamento.FormaPagamento)
                {
                    case "Dinheiro":
                        formaPagamento = FormaPagamento.Dinheiro;
                        break;
                    case "Cheque":
                        formaPagamento = FormaPagamento.Cheque;
                        break;
                    case "Cartão de Crédito":
                        formaPagamento = FormaPagamento.CartaoCredito;
                        break;
                    case "Cartão de Débito":
                        formaPagamento = FormaPagamento.CartaoDebito;
                        break;
                    case "Sem Pagamento":
                        formaPagamento = FormaPagamento.SemPagamento;
                        break;
                    default:
                        throw new ArgumentException();
                }

                pagamentosNf.Add(new Pagamento(formaPagamento, pagamento.QtdeParcelas * pagamento.ValorParcela));
            }

            return pagamentosNf;
        }

        private List<Produto> GetProdutos(NotaFiscalModel notaFiscal, ConfiguracaoEntity config)
        {
            var idsProdutosSelecionados = notaFiscal.Produtos.Select(p => p.ProdutoSelecionado.Id);
            var produtosTo = _produtoRepository.GetAll().Where(p => idsProdutosSelecionados.Contains(p.Id));
            var produtos = new List<Produto>();

            foreach (var produtoNota in notaFiscal.Produtos)
            {
                var produtoEntity = produtosTo.First(c => c.Id == produtoNota.ProdutoSelecionado.Id);

                var impostos = produtoEntity.GrupoImpostos.Impostos.Select(i => new Core.NotasFiscais.Entities.Imposto()
                {
                    Aliquota = i.Aliquota, BaseCalculo = i.BaseCalculo, CST = i.CST, Id = i.Id,
                    GrupoImpostosId = i.GrupoImpostosId, Origem = i.Origem, Reducao = i.Reducao,
                    TipoImposto = i.TipoImposto
                });

                var produto = new Produto(new Impostos(impostos), produtoEntity.Id,
                    produtoEntity.GrupoImpostos.CFOP, produtoEntity.Codigo, produtoEntity.Descricao, produtoEntity.NCM,
                    notaFiscal.Produtos.First(p => p.ProdutoSelecionado.Id == produtoEntity.Id).QtdeProduto,
                    produtoEntity.UnidadeComercial,
                    produtoNota.ValorUnitario, produtoNota.Descontos, _sefazSettings.Ambiente == Ambiente.Homologacao);

                produtos.Add(produto);
            }

            return produtos;
        }

        private static Transporte GetTransporte(NotaFiscalModel notaFiscal, Modelo modeloNota)
        {
            if (modeloNota == Modelo.Modelo65)
            {
                return new Transporte(modeloNota, null, null);
            }

            if (!(notaFiscal is NFeModel nfeModel))
                throw new ArgumentException("Parâmetro NotaFiscal não é do mesmo modelo que modeloNota");

            var transportadora = new Transportadora(nfeModel.TransportadoraSelecionada.CpfCnpj,
                nfeModel.TransportadoraSelecionada.Endereco.ToString(),
                nfeModel.TransportadoraSelecionada.InscricaoEstadual,
                nfeModel.TransportadoraSelecionada.Endereco.Municipio,
                nfeModel.TransportadoraSelecionada.Endereco.UF, nfeModel.TransportadoraSelecionada.NomeRazao);

            var veiculo = new Veiculo(nfeModel.PlacaVeiculo, nfeModel.UfVeiculo);

            return new Transporte(modeloNota, transportadora, veiculo);
        }
    }
}