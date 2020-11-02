using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.Core.UnitTests.Sefaz
{
    public class PisCreatorTest
    {
        [Fact]
        public void Should_create_pis_04()
        {
            var pisCreator = new PisCreator();

            Imposto imposto = new PisOperacaoTributavelMonofasica();
            var detImposto = (TNFeInfNFeDetImpostoPIS)pisCreator.Create(imposto);
            var detPis04 = (TNFeInfNFeDetImpostoPISPISNT)detImposto.Item;

            Assert.Equal(TNFeInfNFeDetImpostoPISPISNTCST.Item04, detPis04.CST);
        }

        [Fact]
        public void Should_create_pis_07()
        {
            var pisCreator = new PisCreator();

            Imposto imposto = new PisOperacaoIsentaContribuicao();
            var detImposto = (TNFeInfNFeDetImpostoPIS)pisCreator.Create(imposto);
            var detPis07 = (TNFeInfNFeDetImpostoPISPISNT)detImposto.Item;

            Assert.Equal(TNFeInfNFeDetImpostoPISPISNTCST.Item07, detPis07.CST);
        }
    }
}
