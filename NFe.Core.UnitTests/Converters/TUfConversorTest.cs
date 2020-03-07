using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using Xunit;
using Retorno = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;

namespace NFe.Core.UnitTests.Converters
{
    public class TUfConversorTest
    {
        [Theory]
        [InlineData("df", TUf.DF)]
        [InlineData("DF", TUf.DF)]
        [InlineData("go", TUf.GO)]
        [InlineData("GO", TUf.GO)]
        [InlineData("mg", TUf.MG)]
        [InlineData("MG", TUf.MG)]
        public void Should_Return_Correct_Enum_Value_When_String_Is_Valid(string uf, TUf expected)
        {
            Assert.Equal(expected, TUfConversor.ToTUf(uf));
        }

        [Theory]
        [InlineData("DF", TUf.DF)]
        [InlineData("GO", TUf.GO)]
        [InlineData("MG", TUf.MG)]
        public void Should_Return_Correct_String_Value_When_Enum_Is_Valid(string expected, TUf entry)
        {
            Assert.Equal(expected, TUfConversor.ToSiglaUf(entry));
        }

        [Theory]
        [InlineData("DF", Retorno.TUf.DF)]
        [InlineData("GO", Retorno.TUf.GO)]
        [InlineData("MG", Retorno.TUf.MG)]
        public void Should_Return_Correct_String_Value_When_Enum_Is_Valid_Other_Type(string expected, Retorno.TUf entry)
        {
            Assert.Equal(expected, TUfConversor.ToSiglaUf(entry));
        }

        [Fact]
        public void Should_Throw_Exception_When_String_Invalid()
        {
            Assert.Throws<InvalidOperationException>(() => TUfConversor.ToTUf("GBO"));
        }
    }
}
