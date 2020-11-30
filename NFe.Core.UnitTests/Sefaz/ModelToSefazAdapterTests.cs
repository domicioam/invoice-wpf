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

            var produto1 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false);
            var produto2 = new Produto(impostos, 0, "1101", "1234", "Produto", "1234", 1, "UN", 125, 0, false);
            List<Produto> produtos = new List<Produto> { produto1, produto2 };

            var result = ModelToSefazAdapter.ConvertIcmsTotal(produtos);

            Assert.Equal("250.00", result.vBC);
            Assert.Equal("25.00", result.vICMS);
        }
    }
}
