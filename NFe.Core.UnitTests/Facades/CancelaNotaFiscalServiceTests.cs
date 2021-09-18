using AutoFixture;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using System;
using System.Globalization;
using Xunit;

namespace NFe.Core.UnitTests.Facades
{
    public class CancelaNotaFiscalServiceTests
    {
        private const string DATE_STRING_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        private RetornoEventoCancelamento ResultadoCancelamentoSucesso => new RetornoEventoCancelamento()
        {
            Status = StatusEvento.SUCESSO,
            DataEvento = new DateTime(20, 10, 20).ToString(DATE_STRING_FORMAT),
            IdEvento = "ID12345667899"
        };

        private RetornoEventoCancelamento ResultadoCancelamentoErro => new RetornoEventoCancelamento()
        {
            Status = StatusEvento.ERRO,
            DataEvento = new DateTime(20, 10, 20).ToString(DATE_STRING_FORMAT),
            IdEvento = "ID12345667899"
        };

        [Fact]
        public void Should_cancel_nota_fiscal_and_save_event_correctly_when_data_is_valid()
        {
            var nfeCancelamento = new Mock<ICancelaNotaFiscalService>();
            nfeCancelamento.Setup(n => n.CancelarNotaFiscal(It.IsAny<DadosNotaParaCancelar>(), It.IsAny<string>()))
                .Returns(ResultadoCancelamentoSucesso);
            
            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            notaFiscalRepository.Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Id = 1 });

            var eventoService = new Mock<IEventoRepository>();
            var cancelaNotaFiscalFacade = new CancelaNotaFiscalService(notaFiscalRepository.Object, eventoService.Object, 
                new Mock<CertificadoService>().Object, new Mock<IServiceFactory>().Object, new Core.Sefaz.SefazSettings());

            var fixture = new Fixture();
            var dadosNotaParaCancelar = fixture.Build<DadosNotaParaCancelar>().Create();

            // Act
            cancelaNotaFiscalFacade.CancelarNotaFiscal(dadosNotaParaCancelar, "Teste Unitário");

            nfeCancelamento.Verify(n => n.CancelarNotaFiscal(It.IsAny<DadosNotaParaCancelar>(), It.IsAny<string>()), Times.Once);
            notaFiscalRepository.Verify(n => n.GetNotaFiscalByChave(It.IsAny<string>()), Times.Once);
            notaFiscalRepository.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), null), Times.Once);

            var expectedEventoEntity = new EventoEntity
            {
                DataEvento = DateTime.ParseExact(ResultadoCancelamentoSucesso.DataEvento, DATE_STRING_FORMAT, CultureInfo.InvariantCulture),
                ChaveIdEvento = ResultadoCancelamentoSucesso.IdEvento.Replace("ID", string.Empty)
            };

            eventoService.Verify(e => e.Salvar(It.Is<EventoEntity>(entity => entity.DataEvento == expectedEventoEntity.DataEvento && entity.ChaveIdEvento == expectedEventoEntity.ChaveIdEvento)), Times.Once);
        }

        [Fact]
        public void Should_not_update_entity_when_cancellation_fails()
        {
            var nfeCancelamento = new Mock<ICancelaNotaFiscalService>();
            nfeCancelamento.Setup(n => n.CancelarNotaFiscal(It.IsAny<DadosNotaParaCancelar>(), It.IsAny<string>()))
                .Returns(ResultadoCancelamentoErro);

            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            var eventoService = new Mock<IEventoRepository>();
            var cancelaNotaFiscalFacade = new CancelaNotaFiscalService(notaFiscalRepository.Object, eventoService.Object,
                new Mock<CertificadoService>().Object, new Mock<IServiceFactory>().Object, new Core.Sefaz.SefazSettings());

            var fixture = new Fixture();
            var dadosNotaParaCancelar = fixture.Build<DadosNotaParaCancelar>().Create();

            // Act
            cancelaNotaFiscalFacade.CancelarNotaFiscal(dadosNotaParaCancelar, "Teste Unitário");

            nfeCancelamento.Verify(n => n.CancelarNotaFiscal(It.IsAny<DadosNotaParaCancelar>(), It.IsAny<string>()), Times.Once);
            notaFiscalRepository.Verify(n => n.GetNotaFiscalByChave(It.IsAny<string>()), Times.Never);
            notaFiscalRepository.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), null), Times.Never);
            eventoService.Verify(e => e.Salvar(It.IsAny<EventoEntity>()), Times.Never);
        }
    }
}
