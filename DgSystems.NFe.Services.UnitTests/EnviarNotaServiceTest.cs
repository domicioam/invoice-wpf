using Moq;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests
{
    public class EnviarNotaServiceTest
    {
        [Fact]
        public void Should_create_enviar_nota_service()
        {
            var configuracaoRepositoryMock = new Mock<IConfiguracaoRepository>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var consultaNotaFiscalServiceMock = new Mock<IConsultarNotaFiscalService>();

            var enviarNotaService = new EnviarNotaFiscalService(configuracaoRepositoryMock.Object, serviceFactoryMock.Object, consultaNotaFiscalServiceMock.Object);
        } 
    }
}
