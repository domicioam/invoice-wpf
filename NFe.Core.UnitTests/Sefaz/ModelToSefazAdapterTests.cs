using System.Collections.Generic;
using NFe.Core;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.NotasFiscais.ValueObjects;
using NFe.Core.Sefaz;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using Xunit;
using Endereco = NFe.Core.Endereco;
using Imposto = NFe.Core.NotasFiscais.Entities.Imposto;

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
            var imposto = new Imposto
            {
                Aliquota = 10, BaseCalculo = 125, CST = "60", Origem = Origem.Nacional, TipoImposto = TipoImposto.Icms
            };
            var impostos_list = new List<Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};

            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, produtos);
            notaFiscal.TotalNFe = new TotalNFe();

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
        }

        [Fact]
        public void test_ConvertIcmsTotal_IcmsDesonerado()
        {
            // IcmsNaoTributado

            var imposto = new Imposto
            {
                Aliquota = 10, BaseCalculo = 125, CST = "41", Origem = Origem.Nacional, TipoImposto = TipoImposto.Icms,
                MotivoDesoneracao = MotivoDesoneracao.Outros, ValorDesonerado = 50
            };
            var impostos_list = new List<Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, produtos);
            notaFiscal.TotalNFe = new TotalNFe();

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("0.00", result.vBC);
            Assert.Equal("0.00", result.vICMS);
            Assert.Equal("100.00", result.vICMSDeson);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Retido_Anteriormente()
        {
            var imposto = new Imposto
            {
                Aliquota = 10, BaseCalculo = 125, CST = "60", Origem = Origem.Nacional, TipoImposto = TipoImposto.Icms,
                AliquotaFCP = 5, BaseCalculoFCP = 60
            };
            var impostos_list = new List<Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, produtos);
            notaFiscal.TotalNFe = new TotalNFe();

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
            Assert.Equal("6.00", result.vFCPSTRet);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Por_ST()
        {
            var imposto = new Imposto
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
            var impostos_list = new List<Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, produtos);
            notaFiscal.TotalNFe = new TotalNFe();

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
            var imposto = new Imposto
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

            var impostos_list = new List<Imposto> {imposto};
            var impostos = new Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produtos = new List<Produto> {produto1, produto2};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, produtos);
            notaFiscal.TotalNFe = new TotalNFe();

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
            var impostos_list = new List<Imposto>();
            impostos_list.Add(new Imposto
            {
                Aliquota = 1, BaseCalculo = 10, TipoImposto = TipoImposto.Cofins, CST = "01", Origem = Origem.Nacional
            });
            impostos_list.Add(new Imposto
                {CST = "60", Aliquota = 2, BaseCalculo = 20, TipoImposto = TipoImposto.Icms, Origem = Origem.Nacional});
            impostos_list.Add(new Imposto {CST = "04", Origem = Origem.Nacional, TipoImposto = TipoImposto.PIS});

            var produto = new Produto(new Impostos(impostos_list), 0, "1101", "1234", "Produto", "1234", 1, "UN", 125,
                0, false, 5, 10, 15);

            var produtos = new List<Produto> {produto};
            var emissor = new Emissor(null, null, null, null, null, null, "RegimeNormal",
                new Endereco(null, null, null, "BRASILIA", null, "DF"), null);

            var notaFiscal = new NotaFiscal(emissor, null, new IdentificacaoNFe(), null, null, null, produtos);
            notaFiscal.TotalNFe = new TotalNFe();

            var result = ModelToSefazAdapter.ConvertIcmsTotal(notaFiscal);


            Assert.Equal("155.00", result.vNF);
        }

        [Fact]
        public void Should_set_correct_fields_when_nota_fiscal_is_valid()
        {
            var tNFe = ModelToSefazAdapter.GetLoteNFe(_fixture.NotaFiscal);

            // Identificação
            var infIde = tNFe.NFe[0].infNFe.ide;
            Assert.Equal(TAmb.Item2, infIde.tpAmb);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.NaturezaOperacao, infIde.natOp);
            Assert.Equal(_fixture.NotaFiscal.Identificacao.Chave.DigitoVerificador.ToString(), infIde.cDV);
        }
    }
}