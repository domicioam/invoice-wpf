using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.Core.UnitTests.Impostos
{
    public class PisOperacaoIsentaContribuicaoTests
    {
        [Fact]
        public void Should_create_pis_correctly()
        {
            var pis = new PisOperacaoIsentaContribuicao();
            Assert.Equal(Pis.CstEnum.CST07, pis.Cst);
        }
    }
}
