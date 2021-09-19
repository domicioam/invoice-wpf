using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests
{
    public class InutilizarNotaFiscalServiceTest
    {
        [Fact]
        public void Should_inutilizar_numbers()
        {
            // Given
            Mock<INotaInutilizadaRepository> notaInutilizadaService = new Mock<INotaInutilizadaRepository>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            var service = new InutilizarNotaFiscalServiceMock(notaInutilizadaService.Object, sefazSettings.Object,
                certificadoService.Object, serviceFactory.Object, "102");

            // When
            var retorno = service.InutilizarNotaFiscal(CodigoUfIbge.DF, "1234", Modelo.Modelo55, "123", "1", "10");
            
            // Then
            Assert.Equal(Status.SUCESSO, retorno.Status);
            Assert.False(string.IsNullOrEmpty(retorno.Xml));
            Assert.False(string.IsNullOrEmpty(retorno.ProtocoloInutilizacao));
            Assert.False(string.IsNullOrEmpty(retorno.Mensagem));
            Assert.False(string.IsNullOrEmpty(retorno.MotivoInutilizacao));
            Assert.False(string.IsNullOrEmpty(retorno.IdInutilizacao));
        }

        [Fact]
        public void Should_return_error_when_status_not_success()
        {
            // Given
            Mock<INotaInutilizadaRepository> notaInutilizadaService = new Mock<INotaInutilizadaRepository>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();

            var service = new InutilizarNotaFiscalServiceMock(notaInutilizadaService.Object, sefazSettings.Object,
                certificadoService.Object, serviceFactory.Object, "500");

            // When
            RetornoInutilizacao retorno = service.InutilizarNotaFiscal(CodigoUfIbge.DF, "1234", Modelo.Modelo55, "123", "1", "10");
            
            // Then
            Assert.Equal(Status.ERRO, retorno.Status);
            Assert.True(string.IsNullOrEmpty(retorno.Xml));
            Assert.True(string.IsNullOrEmpty(retorno.ProtocoloInutilizacao));
            Assert.False(string.IsNullOrEmpty(retorno.Mensagem));
            Assert.True(string.IsNullOrEmpty(retorno.MotivoInutilizacao));
            Assert.True(string.IsNullOrEmpty(retorno.IdInutilizacao));
        }
    }
}
