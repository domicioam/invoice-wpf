using NFe.Core.Cadastro.Imposto;
using NFe.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static NFe.Core.Icms;
using NFe.Core.Domain;
using NFe.Core.NotasFiscal.Impostos;
using DgSystems.NFe.Core.Cadastro;

namespace NFe.Core.UnitTests.Impostos
{
    public class ImpostoFactoryTests
    {
        [Fact]
        public void Should_create_cofins_cumulativo()
        {
            var factory = new ImpostoFactory();
            var input = new NFe.Core.Domain.Imposto()
            {
                BaseCalculo = 10,
                Aliquota = 1,
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Cofins,
                CST = "01"
            };

            NFe.Core.Domain.Interface.Imposto result = factory.CreateImposto(input);

            Assert.True(result is CofinsCumulativoNaoCumulativo);
            var cofins = result as CofinsCumulativoNaoCumulativo;
            Assert.Equal(10m, cofins.BaseCalculo);
            Assert.Equal(1m, cofins.Aliquota);
        }

        [Fact]
        public void Should_fail_when_origem_not_nacional()
        {
            var factory = new ImpostoFactory();
            var input = new NFe.Core.Domain.Imposto();

            Assert.Throws<NotSupportedException>(() => factory.CreateImposto(input));
        }

        [Fact]
        public void Should_fail_when_tipoimposto_out_of_range()
        {
            var factory = new ImpostoFactory();
            var input = new NFe.Core.Domain.Imposto() { Origem = Origem.Nacional };
            Assert.Throws<NotSupportedException>(() => factory.CreateImposto(input));
        }

        [Fact]
        public void Should_create_icms_nao_tributado_without_desoneracao()
        {
            var factory = new ImpostoFactory();
            var input = new NFe.Core.Domain.Imposto()
            {
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Icms,
                CST = "41"
            };

            NFe.Core.Domain.Interface.Imposto result = factory.CreateImposto(input);

            Assert.True(result is IcmsNaoTributado);
            var icms = result as IcmsNaoTributado;
            Assert.Equal(CstEnum.CST41, icms.Cst);
            Assert.Null(icms.Desoneracao);
        }

        [Fact]
        public void Should_create_icms_nao_tributado_with_desoneracao_when_valor_desonerado_bigger_than_0()
        {
            var factory = new ImpostoFactory();
            var input = new NFe.Core.Domain.Imposto()
            {
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Icms,
                CST = "41",
                ValorDesonerado = 1,
                MotivoDesoneracao = MotivoDesoneracao.Outros
            };

            NFe.Core.Domain.Interface.Imposto result = factory.CreateImposto(input);

            Assert.True(result is IcmsNaoTributado);
            var icms = result as IcmsNaoTributado;
            Assert.Equal(CstEnum.CST41, icms.Cst);
            Assert.Equal(1m, icms.Desoneracao.ValorDesonerado);
            Assert.NotNull(icms.Desoneracao.MotivoDesoneracao);
        }

        [Fact]
        public void Should_not_create_icms_nao_tributado_with_desoneracao_when_motivo_desoneracao_invalid()
        {
            var factory = new ImpostoFactory();
            var input = new NFe.Core.Domain.Imposto()
            {
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Icms,
                CST = "41",
                ValorDesonerado = 1
            };

            Assert.Throws<ArgumentException>(() => factory.CreateImposto(input));
        }

        [Fact]
        public void Should_not_create_icms_nao_tributado_with_desoneracao_when_valor_desoneracao_invalid()
        {
            var factory = new ImpostoFactory();
            var input = new global::NFe.Core.Domain.Imposto()
            {
                Origem = Origem.Nacional,
                TipoImposto = TipoImposto.Icms,
                CST = "41",
                ValorDesonerado = 0,
                MotivoDesoneracao = MotivoDesoneracao.Outros
            };

            Assert.Throws<ArgumentException>(() => factory.CreateImposto(input));
        }
    }
}
