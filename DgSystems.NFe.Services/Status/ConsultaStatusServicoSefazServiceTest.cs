using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using System;
using System.Configuration;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests
{
    public class ConsultaStatusServicoSefazServiceTest : IClassFixture<CertificateFixture>
    {
        private readonly CertificateFixture certificateFixture;

        public ConsultaStatusServicoSefazServiceTest(CertificateFixture certificateFixture)
        {
            this.certificateFixture = certificateFixture;
        }

        [Fact]
        public void Should_return_status_true_when_service_online()
        {
            // Given
            var emissorService = new Mock<IEmitenteRepository>();
            var certificadoRepository = new Mock<ICertificadoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            certificadoService.Setup(c => c.GetX509Certificate2()).Returns(certificateFixture.X509Certificate2);
            certificadoRepository.Setup(c => c.GetCertificado()).Returns(new CertificadoEntity());

            emissorService.Setup(e => e.GetEmissor()).Returns(new Emissor() { Endereco = new Endereco() { UF = "DF" } });
            var service = new ConsultaStatusServicoSefazServiceMock(emissorService.Object, certificadoRepository.Object, certificadoService.Object, "107");

            // When
            bool status = service.ExecutarConsultaStatus(Modelo.Modelo55, ConfigurationManager.AppSettings["sefazEnvironment"]);

            // Then
            Assert.True(status);
        }

        [Fact]
        public void Should_return_status_false_when_service_offline()
        {
            // Given
            var emissorService = new Mock<IEmitenteRepository>();
            var certificadoRepository = new Mock<ICertificadoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            certificadoService.Setup(c => c.GetX509Certificate2()).Returns(certificateFixture.X509Certificate2);
            certificadoRepository.Setup(c => c.GetCertificado()).Returns(new CertificadoEntity());

            emissorService.Setup(e => e.GetEmissor()).Returns(new Emissor() { Endereco = new Endereco() { UF = "DF" } });
            var service = new ConsultaStatusServicoSefazServiceMock(emissorService.Object, certificadoRepository.Object, certificadoService.Object, "400");

            // When
            bool status = service.ExecutarConsultaStatus(Modelo.Modelo55, ConfigurationManager.AppSettings["sefazEnvironment"]);

            // Then
            Assert.False(status);
        }

        [Fact]
        public void Should_return_status_false_when_exception_thrown()
        {
            // Given
            var emissorService = new Mock<IEmitenteRepository>();
            var certificadoRepository = new Mock<ICertificadoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            certificadoService.Setup(c => c.GetX509Certificate2()).Returns(certificateFixture.X509Certificate2);
            certificadoRepository.Setup(c => c.GetCertificado()).Throws(new Exception());

            emissorService.Setup(e => e.GetEmissor()).Returns(new Emissor() { Endereco = new Endereco() { UF = "DF" } });
            var service = new ConsultaStatusServicoSefazServiceMock(emissorService.Object, certificadoRepository.Object, certificadoService.Object, "400");

            // When
            bool status = service.ExecutarConsultaStatus(Modelo.Modelo55, ConfigurationManager.AppSettings["sefazEnvironment"]);

            // Then
            Assert.False(status);
        }

        [Fact]
        public void Should_return_status_false_when_certificate_doesnt_exist()
        {
            // Given
            var emissorService = new Mock<IEmitenteRepository>();
            var certificadoRepository = new Mock<ICertificadoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            certificadoService.Setup(c => c.GetX509Certificate2()).Returns(certificateFixture.X509Certificate2);
            emissorService.Setup(e => e.GetEmissor()).Returns(new Emissor() { Endereco = new Endereco() { UF = "DF" } });
            var service = new ConsultaStatusServicoSefazServiceMock(emissorService.Object, certificadoRepository.Object, certificadoService.Object, "400");

            // When
            bool status = service.ExecutarConsultaStatus(Modelo.Modelo55, ConfigurationManager.AppSettings["sefazEnvironment"]);

            // Then
            Assert.False(status);
        }
    }
}
