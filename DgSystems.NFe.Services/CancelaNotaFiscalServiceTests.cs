using AutoMapper;
using DgSystems.NFe.Services.UnitTests;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using System;
using System.Globalization;
using Xunit;

namespace NFe.Core.UnitTests.Facades
{
    public class CancelaNotaFiscalServiceTests : IClassFixture<CertificateFixture>, IClassFixture<CancelaNotaFiscalFixture>
    {
        public CancelaNotaFiscalServiceTests(CertificateFixture fixture, CancelaNotaFiscalFixture cancelaNotaFiscalFixture)
        {
            this.fixture = fixture;
            this.cancelaNotaFiscalFixture = cancelaNotaFiscalFixture;
        }

        private const string DATE_STRING_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly CertificateFixture fixture;
        private readonly CancelaNotaFiscalFixture cancelaNotaFiscalFixture;

        [Fact]
        public void Should_cancel_nota_fiscal_and_save_event_correctly_when_data_is_valid()
        {
            // Given
            var mapper = new Mock<IMapper>();
            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            var eventoService = new Mock<IEventoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            certificadoService.Setup(c => c.GetX509Certificate2()).Returns(fixture.X509Certificate2);

            notaFiscalRepository.Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Id = 1 });

            var cancelaNotaFiscalFacade = new CancelaNotaFiscalServiceMock(notaFiscalRepository.Object, eventoService.Object,
                certificadoService.Object, new Mock<IServiceFactory>().Object, new Sefaz.SefazSettings(), 
                mapper.Object, cancelaNotaFiscalFixture.cStatSucesso, cancelaNotaFiscalFixture.RetEventoSuceso);

            // When
            var resultado = cancelaNotaFiscalFacade.CancelarNotaFiscal(cancelaNotaFiscalFixture.NotaParaCancelar, "Teste Unitário");

            // Then
            notaFiscalRepository.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), null), Times.Once);
            notaFiscalRepository.Verify(n => n.GetNotaFiscalByChave(It.IsAny<string>()), Times.Once);

            eventoService.Verify(e => e.Salvar(
                It.Is<EventoEntity>(entity => 
                    entity.DataEvento == cancelaNotaFiscalFixture.expectedEventoEntity.DataEvento
                    && entity.ChaveIdEvento == cancelaNotaFiscalFixture.expectedEventoEntity.ChaveIdEvento)), Times.Once);

            Assert.Equal(StatusEvento.SUCESSO, resultado.Status);
            Assert.NotNull(resultado.Xml);
            Assert.NotNull(resultado.ProtocoloCancelamento);
            Assert.NotNull(resultado.MotivoCancelamento);
        }

        [Fact]
        public void Should_not_update_entity_when_cancellation_fails()
        {
            // Given
            var mapper = new Mock<IMapper>();
            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            var eventoService = new Mock<IEventoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            certificadoService.Setup(c => c.GetX509Certificate2()).Returns(fixture.X509Certificate2);

            var cancelaNotaFiscalFacade = new CancelaNotaFiscalService(notaFiscalRepository.Object, eventoService.Object,
                certificadoService.Object, new Mock<IServiceFactory>().Object, new SefazSettings(), mapper.Object);

            // When
            var resultado = cancelaNotaFiscalFacade.CancelarNotaFiscal(cancelaNotaFiscalFixture.NotaParaCancelar, "Teste Unitário");

            // Then
            notaFiscalRepository.Verify(n => n.GetNotaFiscalByChave(It.IsAny<string>()), Times.Never);
            notaFiscalRepository.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), null), Times.Never);
            eventoService.Verify(e => e.Salvar(It.IsAny<EventoEntity>()), Times.Never);

            Assert.Equal(StatusEvento.ERRO, resultado.Status);
        }

        [Fact]
        public void Should_return_sucesso_when_nota_cancelada()
        {
            // Given
            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            var eventoService = new Mock<IEventoRepository>();
            var certificadoService = new Mock<CertificadoService>();
            var serviceFactory = new Mock<IServiceFactory>();
            var sefazSettings = new Mock<SefazSettings>();
            var mapper = new Mock<IMapper>();
            notaFiscalRepository.Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Id = 1 });

            var service = new CancelaNotaFiscalServiceMock(notaFiscalRepository.Object, eventoService.Object, certificadoService.Object,
                serviceFactory.Object, sefazSettings.Object, mapper.Object, cancelaNotaFiscalFixture.cStatSucesso, cancelaNotaFiscalFixture.RetEventoSuceso);

            // When
            var resultado = service.CancelarNotaFiscal(cancelaNotaFiscalFixture.NotaParaCancelar, "");

            // Then
            Assert.Equal(StatusEvento.SUCESSO, resultado.Status);
            Assert.NotNull(resultado.Xml);
            Assert.NotNull(resultado.ProtocoloCancelamento);
            Assert.NotNull(resultado.MotivoCancelamento);
        }
    }
}
