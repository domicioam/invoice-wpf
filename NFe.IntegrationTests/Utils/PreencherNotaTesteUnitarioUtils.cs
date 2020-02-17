using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFe.Core.Domain.Services.Configuracao;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Domain.Services.Emissor;
using NFe.Core.Model.Dest;
using NFe.Core.TO;
using NFe.Core.Domain.Services.Produto;
using System.Collections.Generic;
using NFe.Core.Domain.Services.ICMS;
using NFe.Core.Domain.Services.Pagto;
using NFe.Core.Domain.Services;
using NFe.Core.Domain.Services.Transp;
using NFe.Core.Domain.Services.NotaFiscal;
using NFe.Core;
using NFe.Core.Servicos;
using System.Globalization;
using System.IO;
using NFe.Core.Utils.Xml;
using NFe.Core.Domain.Services.Destinatario;
using NFe.Repository;
using System.Linq;
using NFe.Repository.Repositories;

namespace NFe.CoreTest.Utils
{
    static class EnviarNotaTesteUnitarioUtils
    {
        static NFeContext context = new NFeContext();


        public static int EnviarNotaFiscal(out NotaFiscal notaFiscal, out ConfiguracaoEntity config, Destinatario destinatario = null)
        {
            notaFiscal = null;
            config = ConfiguracaoService.GetConfiguracao();
            var ambiente = Ambiente.Homologacao;
            var modeloNota = Modelo.Modelo65;
            var tipoEmissao = TipoEmissao.Normal;

            var documentoDanfe = destinatario != null ? destinatario.DocumentoDanfe : "CPF"; //Encapsular isso aqui

            var emitente = EmissorService.GetEmissor();
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            var serie = config.SerieNFCeHom;
            var numero = config.ProximoNumNFCeHom;

            var identificacao = GetIdentificacao(codigoUF, DateTime.Now, emitente, modeloNota, Convert.ToInt32(serie), numero, tipoEmissao, ambiente, documentoDanfe);
            var produtos = GetProdutos(config);
            var totalNFe = GetTotalNFe();
            var pagamentos = GetPagamentos();
            var infoAdicional = new InfoAdicional(produtos);
            var transporte = new Transporte(modeloNota, null, null);

            notaFiscal = new NFe.Core.TO.NotaFiscal(emitente, destinatario, identificacao, transporte, totalNFe, infoAdicional, produtos, pagamentos);

            string cscId = config.CscIdHom;
            string csc = config.CscHom;
            NotaFiscalService notaService = new NotaFiscalService();

            return notaService.EnviarNotaFiscalAsync(notaFiscal, cscId, csc).Result;
        }

        public static async void SalvarNotaFiscal(NFe.Core.TO.NotaFiscal notaFiscal, RetornoNotaFiscal mensagemRetorno, Ambiente ambiente, int idNota)
        {
            await new NotaFiscalService().SalvarNotaFiscalAsync(notaFiscal, mensagemRetorno, ambiente, idNota);
        }

        private static List<NFe.Core.Model.Produto> GetProdutos(ConfiguracaoEntity config)
        {
            var produtos = new List<NFe.Core.Model.Produto>();

            var ProdutoEntity = ProdutoService.GetByCodigo("0001");
            var grupoImpostos = new GrupoImpostosRepository(context).GetById(ProdutoEntity.GrupoImpostosId);

            var produto = new NFe.Core.Model.Produto(grupoImpostos, ProdutoEntity.Id, grupoImpostos.CFOP, ProdutoEntity.Codigo, ProdutoEntity.Descricao, ProdutoEntity.NCM,
                                            1, ProdutoEntity.UnidadeComercial,
                                            ProdutoEntity.ValorUnitario, 0, !config.IsProducao);

            produtos.Add(produto);

            return produtos;
        }

        private static List<Pagamento> GetPagamentos()
        {
            var pagamentosNF = new List<Pagamento>();
            var ProdutoEntity = ProdutoService.GetByCodigo("0001");
            pagamentosNF.Add(new Pagamento(FormaPagamento.Dinheiro, ProdutoEntity.ValorUnitario));
            return pagamentosNF;
        }

        private static TotalNFe GetTotalNFe()
        {
            double sumValorTotal = 0;
            double sumValorTotalFrete = 0;
            double sumValorTotalSeguro = 0;
            double sumValorTotalDesconto = 0;

            var ProdutoEntity = ProdutoService.GetByCodigo("0001");

            sumValorTotal += ProdutoEntity.ValorUnitario;
            sumValorTotalFrete += 0;
            sumValorTotalSeguro += 0;
            sumValorTotalDesconto += 0;

            var totalNFe = new TotalNFe();
            totalNFe.IcmsTotal = new IcmsTotal();
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

            return totalNFe;
        }

        private static IdentificacaoNFe GetIdentificacao(CodigoUfIbge codigoUF, DateTime now, Emissor emitente, Modelo modeloNota,
            int serie, string numeroNFe, TipoEmissao tipoEmissao, Ambiente ambiente, string documentoDanfe)
        {
            var identificacao = new IdentificacaoNFe(codigoUF, now, emitente.CNPJ, modeloNota, serie, numeroNFe, tipoEmissao, ambiente, emitente,
                "Venda", FinalidadeEmissao.Normal, true, PresencaComprador.Presencial, documentoDanfe);

            return identificacao;
        }
    }
}
