using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EmissorNFe.Model;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.NotasFiscais.ValueObjects;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Acentuacao;
using NFe.Core.Utils.PDF;
using NFe.WPF.Events;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.NotaFiscal.ViewModel;
using Destinatario = NFe.Core.NotasFiscais.Entities.Destinatario;
using Emissor = NFe.Core.NotasFiscais.Emissor;
using Pagamento = NFe.Core.NotasFiscais.Pagamento;
using Produto = NFe.Core.NotasFiscais.Entities.Produto;

namespace DgSystems.NFe.ViewModels
{
    public class EnviarNotaAppService : IEnviarNotaAppService
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEnviaNotaFiscalFacade _enviaNotaFiscalService;
        private readonly IConfiguracaoService _configuracaoService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly SefazSettings _sefazSettings;
        private readonly IEmiteNotaFiscalContingenciaFacade _emiteNotaFiscalContingenciaService;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly XmlUtil _xmlUtil;

        public EnviarNotaAppService(IEnviaNotaFiscalFacade enviaNotaFiscalService, IConfiguracaoService configuracaoService, IProdutoRepository produtoRepository, SefazSettings sefazSettings,
            IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService, INotaFiscalRepository notaFiscalRepository, XmlUtil xmlUtil)
        {
            _enviaNotaFiscalService = enviaNotaFiscalService;
            _configuracaoService = configuracaoService;
            _produtoRepository = produtoRepository;
            _sefazSettings = sefazSettings;
            _emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            _notaFiscalRepository = notaFiscalRepository;
            _xmlUtil = xmlUtil;
        }

        public async Task<NotaFiscal> EnviarNotaAsync(NotaFiscalModel notaFiscalModel, Modelo modelo, Emissor emissor, X509Certificate2 certificado, IDialogService dialogService)
        {
            notaFiscalModel.ValidateModel();

            if (notaFiscalModel.HasErrors)
                throw new NotaFiscalModelHasErrorsException("Nota fiscal contém erros de validação não resolvidos.");

            double valorTotalProdutos = notaFiscalModel.Produtos.Sum(c => c.QtdeProduto * c.ValorUnitario - c.Descontos + c.Outros + c.Frete + c.Seguro);
            double valorTotalPagamentos = notaFiscalModel.Pagamentos.Sum(p => p.QtdeParcelas * p.ValorParcela);
            bool isNotaComPagamento = notaFiscalModel.Pagamentos[0].FormaPagamento != "Sem Pagamento";

            if (isNotaComPagamento && valorTotalProdutos != valorTotalPagamentos)
            {
                await dialogService.ShowError("Valor total da nota não corresponde ao valor de pagamento.",
                "Erro!", "Ok", null);
                throw new ArgumentException("Valor total da nota não corresponde ao valor de pagamento.");
            }

            var config = await _configuracaoService.GetConfiguracaoAsync();
            NotaFiscal notaFiscal = null;

            await Task.Run(() =>
            {
                const TipoEmissao tipoEmissao = TipoEmissao.Normal;
                var destinatario = CreateDestinatario(notaFiscalModel, _sefazSettings.Ambiente, modelo);
                var documentoDanfe = destinatario != null ? destinatario.Documento.GetDocumentoDanfe(destinatario.TipoDestinatario) : "CPF";
                var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emissor.Endereco.UF);

                var identificacao = CreateIdentificacaoNFe(notaFiscalModel, codigoUF, DateTime.Now, emissor, modelo,
                    Convert.ToInt32(notaFiscalModel.Serie), notaFiscalModel.Numero, tipoEmissao, _sefazSettings.Ambiente, documentoDanfe);
                var produtos = GetProdutos(notaFiscalModel);
                var pagamentos = GetPagamentos(notaFiscalModel);
                var totalNFe = GetTotalNFe();
                var infoAdicional = new InfoAdicional(produtos);
                var transporte = GetTransporte(notaFiscalModel, modelo);

                notaFiscal = new NotaFiscal(emissor, destinatario, identificacao, transporte,
                    totalNFe, infoAdicional, produtos, pagamentos);

                var cscId = config.CscId;
                var csc = config.Csc;
                var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

                XmlNFe xmlNFe = new XmlNFe(notaFiscal, nFeNamespaceName, certificado, cscId, csc);

                try
                {
                    if (config.IsContingencia)
                    {
                        notaFiscal = _emiteNotaFiscalContingenciaService.SaveNotaFiscalContingencia(certificado, config, notaFiscal, cscId, csc, nFeNamespaceName);
                    }
                    else
                    {
                        var resultadoEnvio = _enviaNotaFiscalService.EnviarNotaFiscal(notaFiscal, cscId, csc, certificado, xmlNFe);

                        var xmlNFeProc = _xmlUtil.GerarNfeProcXml(resultadoEnvio.Nfe, resultadoEnvio.QrCode, resultadoEnvio.Protocolo);
                        _notaFiscalRepository.Salvar(notaFiscal, xmlNFeProc);
                    }

                    var theEvent = new NotaFiscalEnviadaEvent() { NotaFiscal = notaFiscal };
                    MessagingCenter.Send(this, nameof(NotaFiscalEnviadaEvent), theEvent);
                }
                catch (WebException e)
                {
                    log.Error(e);

                    // Necessário para não tentar enviar a mesma nota como contingência.
                    _configuracaoService.SalvarPróximoNúmeroSérie(notaFiscal.Identificacao.Modelo, notaFiscal.Identificacao.Ambiente);

                    // Stop execution if model 55
                    if (notaFiscal.Identificacao.Modelo == Modelo.Modelo55)
                        throw;

                    var message = GetExceptionMessage(e);
                    PublishInvoiceSentInContigencyModeEvent(notaFiscal, message);

                    notaFiscal = _emiteNotaFiscalContingenciaService.SaveNotaFiscalContingencia(certificado, config, notaFiscal, cscId, csc, nFeNamespaceName);
                }
                catch (Exception e)
                {
                    log.Error(e);

                    _notaFiscalRepository.SalvarXmlNFeComErro(notaFiscal, xmlNFe.XmlNode);
                    notaFiscal.Identificacao.Status = new StatusEnvio(Status.PENDENTE);
                    var xmlProc = _xmlUtil.GerarNfeProcXml(xmlNFe.TNFe, xmlNFe.QrCode);
                    _notaFiscalRepository.Salvar(notaFiscal, xmlProc);
                    throw;
                }
            });

            return notaFiscal;
        }



        public async Task ImprimirNotaFiscal(NotaFiscal notaFiscal)
        {
            await GeradorPDF.GerarPdfNotaFiscal(notaFiscal);
        }

        private void PublishInvoiceSentInContigencyModeEvent(NotaFiscal notaFiscal, string message)
        {
            var theEvent = new NotaFiscalEmitidaEmContingenciaEvent() { justificativa = message, horário = notaFiscal.Identificacao.DataHoraEmissao };
            MessagingCenter.Send(this, nameof(NotaFiscalEmitidaEmContingenciaEvent), theEvent);
        }

        private static string GetExceptionMessage(Exception e)
        {
            return e.InnerException != null ? e.InnerException.Message : e.Message;
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

        [Obsolete("The fields set in this method are ignored.")]
        private static TotalNFe GetTotalNFe()
        {
            var totalNFe = new TotalNFe { IcmsTotal = new IcmsTotal() };
            return totalNFe;
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

        private List<Produto> GetProdutos(NotaFiscalModel notaFiscal)
        {
            var idsProdutosSelecionados = notaFiscal.Produtos.Select(p => p.ProdutoSelecionado.Id);
            var produtosTo = _produtoRepository.GetAll().Where(p => idsProdutosSelecionados.Contains(p.Id));
            var produtos = new List<Produto>();

            foreach (var produtoNota in notaFiscal.Produtos)
            {
                var produtoEntity = produtosTo.First(c => c.Id == produtoNota.ProdutoSelecionado.Id);

                var impostos = produtoEntity.GrupoImpostos.Impostos.Select(i => new global::NFe.Core.NotasFiscais.Entities.Imposto()
                {
                    Aliquota = i.Aliquota,
                    BaseCalculo = i.BaseCalculo,
                    CST = i.CST,
                    Id = i.Id,
                    GrupoImpostosId = i.GrupoImpostosId,
                    Origem = i.Origem,
                    Reducao = i.Reducao,
                    TipoImposto = i.TipoImposto
                });

                var produto = new Produto(new Impostos(impostos), produtoEntity.Id,
                    produtoEntity.GrupoImpostos.CFOP, produtoEntity.Codigo, produtoEntity.Descricao, produtoEntity.NCM,
                    notaFiscal.Produtos.First(p => p.ProdutoSelecionado.Id == produtoEntity.Id).QtdeProduto,
                    produtoEntity.UnidadeComercial,
                    produtoNota.ValorUnitario, produtoNota.Descontos, _sefazSettings.Ambiente == Ambiente.Homologacao, produtoNota.Frete, produtoNota.Seguro, produtoNota.Outros);

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

            if (!(notaFiscal is NFeViewModel nfeModel))
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