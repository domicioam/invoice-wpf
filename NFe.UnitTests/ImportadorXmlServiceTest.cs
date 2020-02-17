using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NFe.Core.Domain.Services;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Domain.Services.NotaFiscal;
using NFe.Core.Domain.Services.TO;
using NFe.Repository;
using NFe.Repository.Entities;
using NFe.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.UnitTests
{
    [TestClass]
    public class ImportadorXmlServiceTest
    {
        [TestMethod]
        public async Task ImportarXmlAsync_ShouldReturnSucess()
        {
            // Arrange

            string path = Path.GetFullPath(@"XmlFiles\xmls.zip");
            var notaFiscalServiceMock = new Mock<NotaFiscalService>();
            var notaFiscalRepositoryMock = new Mock<NotaFiscalRepository>();
            var notaInutilizadaServiceMock = new Mock<NotaInutilizadaService>();
            var eventoServiceMock = new Mock<EventoService>();

            notaFiscalServiceMock.Setup(r => r.SalvarAsync(It.IsAny<NotaFiscalEntity>(), It.IsAny<string>()));
            notaInutilizadaServiceMock.Setup(r => r.Salvar(It.IsAny<NotaInutilizadaTO>(), It.IsAny<string>()));
            eventoServiceMock.Setup(r => r.Salvar(It.IsAny<EventoEntity>()));

            notaFiscalRepositoryMock.Setup(ns => ns.GetNotaFiscalByChave(It.IsAny<string>(), It.IsAny<int>())).Returns(new NotaFiscalEntity());

            var importador = new ImportadorXmlService(notaFiscalServiceMock.Object, notaFiscalRepositoryMock.Object, notaInutilizadaServiceMock.Object, eventoServiceMock.Object);

            // Act

            await importador.ImportarXmlAsync(path);

            //Assert

            notaFiscalServiceMock.Verify(n => n.SalvarAsync(It.IsAny<NotaFiscalEntity>(), It.IsAny<string>()), Times.Exactly(5));
            notaInutilizadaServiceMock.Verify(n => n.Salvar(It.IsAny<NotaInutilizadaTO>(), It.IsAny<string>()), Times.Once);
            eventoServiceMock.Verify(n => n.Salvar(It.IsAny<EventoEntity>()), Times.Exactly(2));
        }
    }
}
