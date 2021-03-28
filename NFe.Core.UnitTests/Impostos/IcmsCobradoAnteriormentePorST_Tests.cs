using NFe.Core.NotaFiscal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.Core.UnitTests
{
    public class IcmsCobradoAnteriormentePorST_Tests
    {
        [Fact]
        public void Should_calculate_using_base_calculo()
        {
            var icms60 = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(18, 65, new NotasFiscais.FundoCombatePobreza(2, 65), OrigemMercadoria.Nacional);
            Assert.Equal(11.7m, icms60.Valor);
            Assert.Equal(1.3m, icms60.FundoCombatePobreza.Valor);
        }
    }
}
