using AutoFixture;
using Moq;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
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

        [Fact]
        public void Should_Cancel_Nota_Fiscal_And_Save_Event_Correctly_When_Data_Is_Valid()
        {
            var nfeCancelamento = new Mock<INFeCancelamento>();

            var date = new DateTime(20, 10, 20);

            MensagemRetornoEventoCancelamento resultadoCancelamento = new MensagemRetornoEventoCancelamento() 
            { 
                Status = StatusEvento.SUCESSO,
                DataEvento = date.ToString(DATE_STRING_FORMAT),
                IdEvento = "ID12345667899"
            };

            nfeCancelamento.Setup(n => n.CancelarNotaFiscal(It.IsAny<string>(), It.IsAny<CodigoUfIbge>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Modelo>(), It.IsAny<string>()))
                .Returns(resultadoCancelamento);
            
            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            notaFiscalRepository.Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Id = 1 });

            var eventoService = new Mock<IEventoService>();

            var cancelaNotaFiscalFacade = new CancelaNotaFiscalService(nfeCancelamento.Object, notaFiscalRepository.Object, eventoService.Object);

            var fixture = new Fixture();

            var dadosNotaParaCancelar = fixture.Build<DadosNotaParaCancelar>().Create();

            cancelaNotaFiscalFacade.CancelarNotaFiscal(dadosNotaParaCancelar, "Teste Unitário");

            nfeCancelamento.Verify(n => n.CancelarNotaFiscal(It.IsAny<string>(), It.IsAny<CodigoUfIbge>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Modelo>(), It.IsAny<string>()), Times.Once);
            notaFiscalRepository.Verify(n => n.GetNotaFiscalByChave(It.IsAny<string>()), Times.Once);
            notaFiscalRepository.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), null), Times.Once);

            var expectedEventoEntity = new EventoEntity
            {
                DataEvento = DateTime.ParseExact(resultadoCancelamento.DataEvento, DATE_STRING_FORMAT, CultureInfo.InvariantCulture),
                ChaveIdEvento = resultadoCancelamento.IdEvento.Replace("ID", string.Empty)
            };

            eventoService.Verify(e => e.Salvar(It.Is<EventoEntity>(entity => entity.DataEvento == expectedEventoEntity.DataEvento && entity.ChaveIdEvento == expectedEventoEntity.ChaveIdEvento)), Times.Once);
        }

        [Fact]
        public void Should_Not_Update_Entity_When_Cancellation_Fails()
        {
            var nfeCancelamento = new Mock<INFeCancelamento>();

            var date = new DateTime(20, 10, 20);

            MensagemRetornoEventoCancelamento resultadoCancelamento = new MensagemRetornoEventoCancelamento()
            {
                Status = StatusEvento.ERRO,
                DataEvento = date.ToString(DATE_STRING_FORMAT),
                IdEvento = "ID12345667899"
            };

            nfeCancelamento.Setup(n => n.CancelarNotaFiscal(It.IsAny<string>(), It.IsAny<CodigoUfIbge>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Modelo>(), It.IsAny<string>()))
                .Returns(resultadoCancelamento);

            var notaFiscalRepository = new Mock<INotaFiscalRepository>();
            var eventoService = new Mock<IEventoService>();
            var cancelaNotaFiscalFacade = new CancelaNotaFiscalService(nfeCancelamento.Object, notaFiscalRepository.Object, eventoService.Object);

            var fixture = new Fixture();
            var dadosNotaParaCancelar = fixture.Build<DadosNotaParaCancelar>().Create();

            cancelaNotaFiscalFacade.CancelarNotaFiscal(dadosNotaParaCancelar, "Teste Unitário");

            nfeCancelamento.Verify(n => n.CancelarNotaFiscal(It.IsAny<string>(), It.IsAny<CodigoUfIbge>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Modelo>(), It.IsAny<string>()), Times.Once);
            notaFiscalRepository.Verify(n => n.GetNotaFiscalByChave(It.IsAny<string>()), Times.Never);
            notaFiscalRepository.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), null), Times.Never);
            eventoService.Verify(e => e.Salvar(It.IsAny<EventoEntity>()), Times.Never);
        }
    }
}
