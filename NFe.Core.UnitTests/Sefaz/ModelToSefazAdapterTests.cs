using NFe.Core.NotasFiscais.Entities;
using NFe.Core.Sefaz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.Core.UnitTests.Sefaz
{
    public class ModelToSefazAdapterTests
    {
        [Fact]
        public void test_ConvertIcmsTotal()
        {

            Imposto imposto = new Imposto { Aliquota = 10, BaseCalculo = 125, CST = "60", Origem = Cadastro.Imposto.Origem.Nacional, TipoImposto = Cadastro.Imposto.TipoImposto.Icms };
            List<Imposto> impostos_list = new List<Imposto> { imposto };
            NotasFiscais.Entities.Impostos impostos = new NotasFiscais.Entities.Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            List<Produto> produtos = new List<Produto> { produto1, produto2 };

            var result = ModelToSefazAdapter.ConvertIcmsTotal(produtos);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
        }

        [Fact]
        public void test_ConvertIcmsTotal_IcmsDesonerado()
        {
            // IcmsNaoTributado

            Imposto imposto = new Imposto { Aliquota = 10, BaseCalculo = 125, CST = "41", Origem = Cadastro.Imposto.Origem.Nacional, TipoImposto = Cadastro.Imposto.TipoImposto.Icms, MotivoDesoneracao = MotivoDesoneracao.Outros, ValorDesonerado = 50 };
            List<Imposto> impostos_list = new List<Imposto> { imposto };
            NotasFiscais.Entities.Impostos impostos = new NotasFiscais.Entities.Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            List<Produto> produtos = new List<Produto> { produto1, produto2 };

            var result = ModelToSefazAdapter.ConvertIcmsTotal(produtos);

            Assert.Equal("0.00", result.vBC);
            Assert.Equal("0.00", result.vICMS);
            Assert.Equal("100.00", result.vICMSDeson);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Retido_Anteriormente()
        {
            Imposto imposto = new Imposto { Aliquota = 10, BaseCalculo = 125, CST = "60", Origem = Cadastro.Imposto.Origem.Nacional, TipoImposto = Cadastro.Imposto.TipoImposto.Icms, AliquotaFCP = 5, BaseCalculoFCP = 60 };
            List<Imposto> impostos_list = new List<Imposto> { imposto };
            NotasFiscais.Entities.Impostos impostos = new NotasFiscais.Entities.Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            List<Produto> produtos = new List<Produto> { produto1, produto2 };

            var result = ModelToSefazAdapter.ConvertIcmsTotal(produtos);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
            Assert.Equal("6.00", result.vFCPSTRet);
        }

        [Fact]
        public void test_ConvertIcmsTotal_Fundo_Combate_Pobreza_Por_ST()
        {
            Imposto imposto = new Imposto
            {
                Aliquota = 0,
                BaseCalculo = 0,
                AliquotaST = 10,
                BaseCalculoST = 125,
                CST = "10",
                Origem = Cadastro.Imposto.Origem.Nacional,
                TipoImposto = Cadastro.Imposto.TipoImposto.Icms,
                AliquotaFCP = 5,
                BaseCalculoFCP = 60
            };
            List<Imposto> impostos_list = new List<Imposto> { imposto };
            NotasFiscais.Entities.Impostos impostos = new NotasFiscais.Entities.Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            List<Produto> produtos = new List<Produto> { produto1, produto2 };

            var result = ModelToSefazAdapter.ConvertIcmsTotal(produtos);

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
            Imposto imposto = new Imposto
            {
                Aliquota = 10,
                BaseCalculo = 125,
                AliquotaST = 7,
                BaseCalculoST = 100,
                CST = "10",
                Origem = Cadastro.Imposto.Origem.Nacional,
                TipoImposto = Cadastro.Imposto.TipoImposto.Icms,
                AliquotaFCP = 5,
                BaseCalculoFCP = 60
            };

            List<Imposto> impostos_list = new List<Imposto> { imposto };
            NotasFiscais.Entities.Impostos impostos = new NotasFiscais.Entities.Impostos(impostos_list);

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 0, 0, 0);
            List<Produto> produtos = new List<Produto> { produto1, produto2 };

            var result = ModelToSefazAdapter.ConvertIcmsTotal(produtos);

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
            impostos_list.Add(new Imposto { Aliquota = 1, BaseCalculo = 10, TipoImposto = Cadastro.Imposto.TipoImposto.Cofins, CST = "01", Origem = Cadastro.Imposto.Origem.Nacional });
            impostos_list.Add(new Imposto { CST = "60", Aliquota = 2, BaseCalculo = 20, TipoImposto = Cadastro.Imposto.TipoImposto.Icms, Origem = Cadastro.Imposto.Origem.Nacional }); 
            impostos_list.Add(new Imposto { CST = "04", Origem = Cadastro.Imposto.Origem.Nacional, TipoImposto = Cadastro.Imposto.TipoImposto.PIS });

            var produto = new Produto(new NotasFiscais.Entities.Impostos(impostos_list), 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false, 5, 10, 15); 

            var result = ModelToSefazAdapter.ConvertIcmsTotal(new List<Produto> { produto });

            Assert.Equal("155.00", result.vNF);
        }
    }
}
