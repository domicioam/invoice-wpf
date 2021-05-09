using System.Collections.Generic;
using DgSystems.NFe.Extensions;
using NFe.Core;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Domain;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Conversores.Enums.Autorizacao;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using Xunit;
using DgSystems.NFe.Core.Cadastro;

namespace DgSystems.NFe.Core.UnitTests.Sefaz
{
    public class ModelToSefazAdapterTests : IClassFixture<NotaFiscalFixture>
    {
        private NotaFiscalFixture _fixture;

        public ModelToSefazAdapterTests(NotaFiscalFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void test_ConvertIcmsTotal()
        {
            var imposto = new global::NFe.Core.Domain.Imposto
            {
                Aliquota = 10, BaseCalculo = 125, CST = "60", Origem = Origem.Nacional, TipoImposto = TipoImposto.Icms
            };
            var impostos_list = new List<global::NFe.Core.Domain.Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};

            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, null,null, produtos);

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
        }

        [Fact]
        public void test_ConvertIcmsTotal_IcmsDesonerado()
        {
            // IcmsNaoTributado

            var imposto = new global::NFe.Core.Domain.Imposto
            {
                Aliquota = 10, BaseCalculo = 125, CST = "41", Origem = Origem.Nacional, TipoImposto = TipoImposto.Icms,
                MotivoDesoneracao = MotivoDesoneracao.Outros, ValorDesonerado = 50
            };
            var impostos_list = new List<global::NFe.Core.Domain.Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, null,null, produtos);

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("0.00", result.vBC);
            Assert.Equal("0.00", result.vICMS);
            Assert.Equal("100.00", result.vICMSDeson);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Retido_Anteriormente()
        {
            var imposto = new global::NFe.Core.Domain.Imposto
            {
                Aliquota = 10, BaseCalculo = 125, CST = "60", Origem = Origem.Nacional, TipoImposto = TipoImposto.Icms,
                AliquotaFCP = 5, BaseCalculoFCP = 60
            };
            var impostos_list = new List<global::NFe.Core.Domain.Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null,null,null, produtos);

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
            Assert.Equal("6.00", result.vFCPSTRet);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Por_ST()
        {
            var imposto = new global::NFe.Core.Domain.Imposto
            {
                Aliquota = 0,
                BaseCalculo = 0,
                AliquotaST = 10,
                BaseCalculoST = 125,
                CST = "10",
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Icms,
                AliquotaFCP = 5,
                BaseCalculoFCP = 60
            };
            var impostos_list = new List<global::NFe.Core.Domain.Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, null,null,produtos);

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("0.00", result.vBC);
            Assert.Equal("0.00", result.vICMS);
            Assert.Equal("0.00", result.vFCPSTRet);
            Assert.Equal("6.00", result.vFCPST);
            Assert.Equal("250.00", result.vBCST);
            Assert.Equal("25.00", result.vST);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Com_E_Sem_ST()
        {
            var imposto = new global::NFe.Core.Domain.Imposto
            {
                Aliquota = 10,
                BaseCalculo = 125,
                AliquotaST = 7,
                BaseCalculoST = 100,
                CST = "10",
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Icms,
                AliquotaFCP = 5,
                BaseCalculoFCP = 60
            };

            var impostos_list = new List<global::NFe.Core.Domain.Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null,null,null, produtos);

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
            Assert.Equal("0.00", result.vFCPSTRet);
            Assert.Equal("6.00", result.vFCPST);
            Assert.Equal("200.00", result.vBCST);
            Assert.Equal("14.00", result.vST);
            Assert.Equal("6.00", result.vFCP);
            Assert.Equal("270.00", result.vNF);
        }

        [Fact]
        public void Should_Calculate_Total_Correctly_When_Imposto_not_in_total_calculation()
        {
            var impostos_list = new List<global::NFe.Core.Domain.Imposto>();
            impostos_list.Add(new global::NFe.Core.Domain.Imposto
            {
                Aliquota = 1, BaseCalculo = 10, TipoImposto = TipoImposto.Cofins, CST = "01", Origem = Origem.Nacional
            });
            impostos_list.Add(new global::NFe.Core.Domain.Imposto
            { CST = "60", Aliquota = 2, BaseCalculo = 20, TipoImposto = TipoImposto.Icms, Origem = Origem.Nacional});
            impostos_list.Add(new global::NFe.Core.Domain.Imposto { CST = "04", Origem = Origem.Nacional, TipoImposto = TipoImposto.PIS});

            var produto = new Produto(new Impostos(impostos_list), 0, "1101", "1234", "Produto", "1234", 1, "UN", 125,
                0, false, 5, 10, 15);

            var produtos = new List<Produto> {produto};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null,null,null, produtos);

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);


            Assert.Equal("155.00", result.vNF);
        }

        [Fact]
        public void Should_set_correct_fields_when_nota_fiscal_is_valid()
        {
            var tNFe = ModelToSefazAdapter.GetLoteNFe(_fixture.NotaFiscal);

            // Identificação
            var infIde = tNFe.NFe[0].infNFe.ide;
            Assert.Equal(_fixture.NotaFiscal.Identificacao.UF.ToTCodUfIBGE(), infIde.cUF);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.Numero, infIde.nNF);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.Chave.Codigo, infIde.cNF);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.NaturezaOperacao, infIde.natOp);
            Assert.Equal((TMod)(int)_fixture.NotaFiscal.Identificacao.Modelo, infIde.mod);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.Serie.ToString(), infIde.serie);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.DataHoraEmissao.ToUtcFormatedString(), infIde.dhEmi);
            Assert.Equal((TNFeInfNFeIdeTpNF)(int)_fixture.NotaFiscal.Identificacao.TipoOperacao, infIde.tpNF);
            Assert.Equal(TNFeInfNFeIdeIdDest.Item1, infIde.idDest);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.CodigoMunicipio, infIde.cMunFG);
            Assert.Equal(TNFeInfNFeIdeTpImp.Item4, infIde.tpImp);
            Assert.Equal(TNFeInfNFeIdeTpEmis.Item1, infIde.tpEmis);
            Assert.Equal(TAmb.Item2, infIde.tpAmb);
            Assert.Equal(TFinNFe.Item1, infIde.finNFe); 
            Assert.Equal((TNFeInfNFeIdeIndFinal)(int)_fixture.NotaFiscal.Identificacao.FinalidadeConsumidor, infIde.indFinal);
            Assert.Equal(TNFeInfNFeIdeIndPres.Item1 , infIde.indPres); // better create another nota fiscal specific to this unit test to not depend on external changes to NotaFiscal object
            Assert.Equal((TProcEmi)(int)_fixture.NotaFiscal.Identificacao.ProcessoEmissao, infIde.procEmi);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.VersaoAplicativo, infIde.verProc);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.Chave.DigitoVerificador.ToString(), infIde.cDV);

            // Emitente
            var emit = tNFe.NFe[0].infNFe.emit;
            Assert.Equal(_fixture.NotaFiscal.Emitente.CNAE, emit.CNAE);
            Assert.Equal(_fixture.NotaFiscal.Emitente.CRT, emit.CRT);
            Assert.Equal(_fixture.NotaFiscal.Emitente.InscricaoEstadual, emit.IE);
            Assert.Equal(_fixture.NotaFiscal.Emitente.InscricaoMunicipal, emit.IM);
            Assert.Equal(_fixture.NotaFiscal.Emitente.Nome, emit.xNome);
            Assert.Equal(_fixture.NotaFiscal.Emitente.NomeFantasia, emit.xFant);
            Assert.Equal(_fixture.NotaFiscal.Emitente.Telefone, emit.enderEmit.fone);
            // Falta endereço
        }
    }
}