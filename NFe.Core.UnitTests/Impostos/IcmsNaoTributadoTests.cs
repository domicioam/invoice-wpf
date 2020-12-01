using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.Core.UnitTests
{
    public class IcmsNaoTributadoTests
    {
        [Fact]
        public void Should_create_correctly_when_fields_valid()
        {
            var desoneracao = new Desoneracao(10, MotivoDesoneracao.ProdutorAgropecuario);
            var icms41 = new IcmsNaoTributado(desoneracao, OrigemMercadoria.Nacional,0);

            Assert.Equal(Icms.CstEnum.CST41, icms41.Cst);
            Assert.Equal(OrigemMercadoria.Nacional, icms41.Origem);
            Assert.Equal(10, icms41.Desoneracao.ValorDesonerado);
        }
    }
}
