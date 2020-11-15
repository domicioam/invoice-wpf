using NFe.Core.NotasFiscais;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static NFe.Core.CofinsBase;

namespace NFe.Core.UnitTests.Impostos
{
    public class CofinsAliquotaNormalTests
    {
        [Fact]
        public void Should_create_cofins_correctly_when_fields_are_valid()
        {
            var cofinsAliqNormal = new CofinsAliquotaNormal(65, 10);
            Assert.Equal(6.5m, cofinsAliqNormal.Valor);
            Assert.Equal(CstEnum.CST01, cofinsAliqNormal.Cst);
        }
    }
}
