using NFe.Core.NotaFiscal;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Utility;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NFe.Core.NotaFiscal;

namespace NFe.Core.UnitTests.Sefaz
{
    public class IcmsCreatorTests
    {
        [Fact]
        public void Should_create_icms_60_with_empty_fields()
        {
            var icmsCreator = new IcmsCreator();

            NotaFiscal.Interface.Imposto imposto = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(0, 0, new NotasFiscais.FundoCombatePobreza(0, 0), OrigemMercadoria.Nacional);
            var detImposto = (TNFeInfNFeDetImpostoICMS) icmsCreator.Create(imposto);
            var detImpostoIcms60 = (TNFeInfNFeDetImpostoICMSICMS60) detImposto.Item;

            Assert.Null(detImpostoIcms60.pFCPSTRet);
            Assert.Null(detImpostoIcms60.pST);
            Assert.Null(detImpostoIcms60.vBCFCPSTRet);
            Assert.Null(detImpostoIcms60.vBCSTRet);
            Assert.Null(detImpostoIcms60.vFCPSTRet);
            Assert.Null(detImpostoIcms60.vICMSSTRet);
            Assert.Equal(Torig.Item0, detImpostoIcms60.orig);
            Assert.Equal(TNFeInfNFeDetImpostoICMSICMS60CST.Item60, detImpostoIcms60.CST);
        }

        [Theory]
        [InlineData(18, 65, 2, 65)]
        public void Should_create_icms_60_national_origin_with_corect_values(decimal aliquota, decimal baseCalculo, decimal percentualFundoCombatePobreza, decimal baseCalculoFundoCombatePobreza)
        {
            var icmsCreator = new IcmsCreator();
            OrigemMercadoria origem = OrigemMercadoria.Nacional;

            var imposto = new IcmsCobradoAnteriormentePorSubstituicaoTributaria(aliquota, baseCalculo, new NotasFiscais.FundoCombatePobreza(percentualFundoCombatePobreza, baseCalculoFundoCombatePobreza), origem);
            var detImposto = (TNFeInfNFeDetImpostoICMS)icmsCreator.Create(imposto);
            var detImpostoIcms60 = (TNFeInfNFeDetImpostoICMSICMS60)detImposto.Item;

            Assert.Equal(baseCalculo.ToString(), detImpostoIcms60.vBCSTRet);
            Assert.Equal(aliquota.ToString(), detImpostoIcms60.pST);
            Assert.Equal("11.70", detImpostoIcms60.vICMSSTRet);
            Assert.Equal(baseCalculoFundoCombatePobreza.ToString(), detImpostoIcms60.vBCFCPSTRet);
            Assert.Equal(percentualFundoCombatePobreza.ToString(), detImpostoIcms60.pFCPSTRet);
            Assert.Equal("1.30", detImpostoIcms60.vFCPSTRet);
            Assert.Equal(origem, detImpostoIcms60.orig);
            Assert.Equal(imposto.Cst, detImpostoIcms60.CST);
            Assert.Equal(TNFeInfNFeDetImpostoICMSICMS60CST.Item60, detImpostoIcms60.CST);
        }

        [Fact]
        public void Should_create_icms_41_with_empty_fields()
        {
            var icmsCreator = new IcmsCreator();
            NotaFiscal.Interface.Imposto imposto = new IcmsNaoTributado(null, OrigemMercadoria.Nacional,0);
            var detImposto = (TNFeInfNFeDetImpostoICMS)icmsCreator.Create(imposto);
            var detImpostoIcms40 = (TNFeInfNFeDetImpostoICMSICMS40)detImposto.Item;

            Assert.Equal(TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item1, detImpostoIcms40.motDesICMS);
            Assert.Null(detImpostoIcms40.vICMSDeson);
            Assert.Equal(Torig.Item0, detImpostoIcms40.orig);
            Assert.Equal(TNFeInfNFeDetImpostoICMSICMS40CST.Item41, detImpostoIcms40.CST);
        }

        [Theory]
        [InlineData(12)]
        public void Should_create_icms_41_national_origin_with_corect_values(decimal valorDesoneracao)
        {
            var icmsCreator = new IcmsCreator();
            var desoneracaoIcms = new Desoneracao(valorDesoneracao, MotivoDesoneracao.Outros);
            NotaFiscal.Interface.Imposto imposto = new IcmsNaoTributado(desoneracaoIcms, OrigemMercadoria.Nacional,0);
            var detImposto = (TNFeInfNFeDetImpostoICMS)icmsCreator.Create(imposto);
            var detImpostoIcms40 = (TNFeInfNFeDetImpostoICMSICMS40)detImposto.Item;

            Assert.Equal(TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item9, detImpostoIcms40.motDesICMS);
            Assert.Equal(valorDesoneracao.ToString(), detImpostoIcms40.vICMSDeson);
            Assert.Equal(Torig.Item0, detImpostoIcms40.orig);
            Assert.Equal(TNFeInfNFeDetImpostoICMSICMS40CST.Item41, detImpostoIcms40.CST);
        }

        [Fact]
        public void Should_throw_exception_when_icms_type_is_wrong()
        {
            var icmsCreator = new IcmsCreator();
            NotaFiscal.Interface.Imposto imposto = new CofinsCumulativoNaoCumulativo(0, 0);

            Assert.Throws<ArgumentException>(() => icmsCreator.Create(imposto));
        }
    }
}
