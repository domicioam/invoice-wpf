using Moq;
using NFe.Core.Domain;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests
{
    public class ConsultarNotaFiscalServiceTest
    {
        [Theory]
        [InlineData(Modelo.Modelo55)]
        [InlineData(Modelo.Modelo65)]
        public void Should_return_enviada_when_status_code_100(Modelo modelo)
        {
            // Given
            var sefazSettings = new SefazSettings { Ambiente = Ambiente.Homologacao };
            var consultaService = new ConsultarNotaFiscalServiceMock(sefazSettings, "100");

            // When
            MensagemRetornoConsulta retorno = consultaService.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), X509Certificate2, modelo);

            // Then
            Assert.True(retorno.IsEnviada);
            Assert.NotNull(retorno.Protocolo);
        }

        [Theory]
        [InlineData(Modelo.Modelo55)]
        [InlineData(Modelo.Modelo65)]
        public void Should_not_return_enviada_when_status_code_not_100(Modelo modelo)
        {
            // Given
            var sefazSettings = new SefazSettings { Ambiente = Ambiente.Homologacao };
            var consultaService = new ConsultarNotaFiscalServiceMock(sefazSettings, "500");

            // When
            MensagemRetornoConsulta retorno = consultaService.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), X509Certificate2, modelo);

            // Then
            Assert.False(retorno.IsEnviada);
            Assert.Null(retorno.Protocolo);
        }

        public X509Certificate2 X509Certificate2
        {
            get
            {
                return new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            }
        }
    }
}
