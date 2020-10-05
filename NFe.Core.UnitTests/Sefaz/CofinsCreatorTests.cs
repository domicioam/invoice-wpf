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
    public class CofinsCreatorTests
    {
        [Fact]
        public void Should_create_cofins_aliquota_with_empty_fields()
        {
            var icmsCreator = new CofinsCreator();

            Imposto imposto = new CofinsCumulativoNaoCumulativo(0, 0);
            var detImposto = (TNFeInfNFeDetImpostoCOFINS)icmsCreator.Create(imposto);
            var detCofins01 = (TNFeInfNFeDetImpostoCOFINSCOFINSAliq)detImposto.Item;

            Assert.Null(detCofins01.pCOFINS);
            Assert.Null(detCofins01.vBC);
            Assert.Null(detCofins01.vCOFINS);
            Assert.Equal(TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item01, detCofins01.CST);
        }

        [Theory]
        [InlineData(0.1, 0.2)]
        public void Should_create_cofins_aliquota_with_correct_values(decimal baseCalculo, decimal aliquota)
        {
            var cofinsCreator = new CofinsCreator();

            Imposto imposto = new CofinsCumulativoNaoCumulativo(baseCalculo, aliquota);
            var detImposto = (TNFeInfNFeDetImpostoCOFINS)cofinsCreator.Create(imposto);
            var detCofins01 = (TNFeInfNFeDetImpostoCOFINSCOFINSAliq)detImposto.Item;

            Assert.Equal(baseCalculo.ToString(), detCofins01.vBC);
            Assert.Equal(aliquota.ToString(), detCofins01.pCOFINS);
            Assert.Equal(TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item01, detCofins01.CST);
        }
    }
}
