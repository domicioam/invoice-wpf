using NFe.Core.NotasFiscais;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace NFe.Core.UnitTests.NotasFiscais.Impostos
{
    public class IcmsCobradoAnteriormentePorSubstituicaoTributariaTests
    {
        [Fact]
        public void Should_return_value_0()
        {
            var icms = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(65, new BaseCalculoIcms(65),0,0,new BaseCalculoFundoCombatePobreza(65), OrigemMercadoria.Nacional);
            Assert.Equal(0, icms.Valor);
        }

        [Fact]
        public void Should_return_value_10()
        {
            var icms = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(100, new BaseCalculoIcms(100), 10, 0, new BaseCalculoFundoCombatePobreza(100), OrigemMercadoria.Nacional);
            Assert.Equal(10, icms.Valor);
        }

        [Fact]
        public void Should_return_value_20()
        {
            var icms = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(200, new BaseCalculoIcms(200), 10, 0, new BaseCalculoFundoCombatePobreza(200), OrigemMercadoria.Nacional);
            Assert.Equal(20, icms.Valor);
        }
    }
}
