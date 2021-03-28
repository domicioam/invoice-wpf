using NFe.Core.NotaFiscal;
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

            NFe.Core.NotaFiscal.Interface.Imposto imposto = new CofinsCumulativoNaoCumulativo(0, 0);
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

            NotaFiscal.Interface.Imposto imposto = new CofinsCumulativoNaoCumulativo(baseCalculo, aliquota);
            var detImposto = (TNFeInfNFeDetImpostoCOFINS)cofinsCreator.Create(imposto);
            var detCofins01 = (TNFeInfNFeDetImpostoCOFINSCOFINSAliq)detImposto.Item;

            Assert.Equal(baseCalculo.ToString(), detCofins01.vBC);
            Assert.Equal(aliquota.ToString(), detCofins01.pCOFINS);
            Assert.Equal(TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item01, detCofins01.CST);
        }

        [Fact]
        public void Should_create_cofins_aliquota_diferenciada_with_empty_fields()
        {
            var icmsCreator = new CofinsCreator();

            NotaFiscal.Interface.Imposto imposto = new CofinsAliquotaDiferenciada(0, 0);
            var detImposto = (TNFeInfNFeDetImpostoCOFINS)icmsCreator.Create(imposto);
            var detCofins02 = (TNFeInfNFeDetImpostoCOFINSCOFINSAliq)detImposto.Item;

            Assert.Null(detCofins02.pCOFINS);
            Assert.Null(detCofins02.vBC);
            Assert.Null(detCofins02.vCOFINS);
            Assert.Equal(TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item02, detCofins02.CST);
        }

        [Theory]
        [InlineData(0.1, 0.2)]
        public void Should_create_cofins_aliquota_diferenciada_with_correct_values(decimal baseCalculo, decimal aliquota)
        {
            var cofinsCreator = new CofinsCreator();

            NotaFiscal.Interface.Imposto imposto = new CofinsAliquotaDiferenciada(baseCalculo, aliquota);
            var detImposto = (TNFeInfNFeDetImpostoCOFINS)cofinsCreator.Create(imposto);
            var detCofins02 = (TNFeInfNFeDetImpostoCOFINSCOFINSAliq)detImposto.Item;

            Assert.Equal(baseCalculo.ToString(), detCofins02.vBC);
            Assert.Equal(aliquota.ToString(), detCofins02.pCOFINS);
            Assert.Equal(TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item02, detCofins02.CST);
        }

        [Fact]
        public void Should_throw_exception_when_cofins_type_is_wrong()
        {
            var cofinsCreator = new CofinsCreator();
            NotaFiscal.Interface.Imposto imposto = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(0,0,new NotasFiscais.FundoCombatePobreza(0,0), OrigemMercadoria.Nacional);

            Assert.Throws<ArgumentException>(() => cofinsCreator.Create(imposto));
        }
    }
}
