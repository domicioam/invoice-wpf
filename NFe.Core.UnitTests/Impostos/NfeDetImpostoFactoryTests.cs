using Moq;
using NFe.Core.Sefaz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.Core.UnitTests.Impostos
{
    public class NfeDetImpostoFactoryTests
    {
        [Fact]
        public void test()
        {
            var mock = new Mock<ImpostoCreatorFactory>().Object;
            var factory = new NfeDetImpostoFactory(mock);
        }
    }
}
