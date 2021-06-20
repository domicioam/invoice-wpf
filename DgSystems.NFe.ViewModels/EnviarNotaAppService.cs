using GalaSoft.MvvmLight.Views;
using MediatR;
using NFe.Core;
using NFe.Core.Cadastro.Ibpt;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Acentuacao;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using NFe.WPF.Events;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.NotaFiscal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using NfeProc = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using TNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFe;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;

namespace DgSystems.NFe.ViewModels
{
    public class EnviarNotaAppService : IEnviarNotaAppService
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEnviaNotaFiscalService _enviaNotaFiscalService;
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly SefazSettings _sefazSettings;
        private readonly IEmiteNotaFiscalContingenciaFacade _emiteNotaFiscalContingenciaService;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly IIbptManager _ibptManager;
        private readonly IMediator mediator;

        public EnviarNotaAppService(IEnviaNotaFiscalService enviaNotaFiscalService, IConfiguracaoRepository configuracaoService, IProdutoRepository produtoRepository, SefazSettings sefazSettings,
            IEmiteNotaFiscalContingenciaFacade emiteNotaFiscalContingenciaService, INotaFiscalRepository notaFiscalRepository, IIbptManager ibptManager, IMediator mediator)
        {
            _enviaNotaFiscalService = enviaNotaFiscalService;
            _configuracaoService = configuracaoService;
            _produtoRepository = produtoRepository;
            _sefazSettings = sefazSettings;
            _emiteNotaFiscalContingenciaService = emiteNotaFiscalContingenciaService;
            _notaFiscalRepository = notaFiscalRepository;
            _ibptManager = ibptManager;
            this.mediator = mediator;
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
                var destinatario = Destinatario.CreateDestinatario(notaFiscalModel.DestinatarioSelecionado.Endereco.Logradouro,
                    notaFiscalModel.DestinatarioSelecionado.InscricaoEstadual,
                    notaFiscalModel.DestinatarioSelecionado.NomeRazao,
                    notaFiscalModel.DestinatarioSelecionado.Telefone,
                    notaFiscalModel.DestinatarioSelecionado.Email,
                    notaFiscalModel.DestinatarioSelecionado.Documento ?? notaFiscalModel.Documento,
                    notaFiscalModel.DestinatarioSelecionado.Endereco.Numero,
                    notaFiscalModel.DestinatarioSelecionado.Endereco.Bairro,
                    notaFiscalModel.DestinatarioSelecionado.Endereco.Municipio,
                    notaFiscalModel.DestinatarioSelecionado.Endereco.CEP,
                    notaFiscalModel.DestinatarioSelecionado.Endereco.UF,
                    notaFiscalModel.IsEstrangeiro,
                    _sefazSettings.Ambiente,
                    modelo);

                var documentoDanfe = destinatario != null ? destinatario.Documento.GetDocumentoDanfe(destinatario.TipoDestinatario) : "CPF";
                var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emissor.Endereco.UF);

                var identificacao = CreateIdentificacaoNFe(notaFiscalModel, codigoUF, DateTime.Now, emissor, modelo,
                    Convert.ToInt32(notaFiscalModel.Serie), notaFiscalModel.Numero, tipoEmissao, _sefazSettings.Ambiente, documentoDanfe);
                var produtos = GetProdutos(notaFiscalModel);
                var pagamentos = GetPagamentos(notaFiscalModel);
                var totalNFe = GetTotalNFe();
                var infoAdicional = new InfoAdicional(produtos, _ibptManager);
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

                        var xmlNFeProc = GerarNfeProcXml(resultadoEnvio.Nfe, resultadoEnvio.QrCode, resultadoEnvio.Protocolo);
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
                    var xmlProc = GerarNfeProcXml(xmlNFe.TNFe, xmlNFe.QrCode);
                    _notaFiscalRepository.Salvar(notaFiscal, xmlProc);
                    throw;
                }
            });

            return notaFiscal;
        }

        public virtual string GerarNfeProcXml(TNFe nfe, QrCode urlQrCode, TProtNFe protocolo = null)
        {
            var nfeProc = new TNfeProc();
            const string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            nfeProc.NFe = nfe.ToTNFeRetorno(nFeNamespaceName);

            if (nfeProc.NFe.infNFeSupl != null) nfeProc.NFe.infNFeSupl.qrCode = "";

            if (protocolo != null)
            {
                var protocoloSerializado = XmlUtil.Serialize(protocolo, nFeNamespaceName);
                nfeProc.protNFe = (NfeProc.TProtNFe)XmlUtil.Deserialize<NfeProc.TProtNFe>(protocoloSerializado);
            }
            else
            {
                nfeProc.protNFe = new NfeProc.TProtNFe();
            }

            nfeProc.versao = "4.00";
            var result = XmlUtil.Serialize(nfeProc, nFeNamespaceName).Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            if (nfeProc.NFe.infNFeSupl != null)
            {
                return result.Replace("<qrCode />", "<qrCode>" + urlQrCode + "</qrCode>")
                   .Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            }
            else
            {
                return result.Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            }
        }

        public void ImprimirNotaFiscal(NotaFiscal notaFiscal)
        {
            var command = new ImprimirDanfeCommand(notaFiscal, mediator);
            command.ExecuteAsync();
            if(!command.IsExecuted)
            {
                log.Error("Danfe não impresso.");
            }
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

                IEnumerable<global::NFe.Core.Domain.Imposto> impostos = produtoEntity.GrupoImpostos.Impostos.Select(i => new global::NFe.Core.Domain.Imposto()
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