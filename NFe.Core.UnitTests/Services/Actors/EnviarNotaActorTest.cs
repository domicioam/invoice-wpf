using Akka.Actor;
using Akka.TestKit.Xunit2;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz.Facades;
using System;
using Xunit;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EnviarNotaActorTest : TestKit, IClassFixture<NotaFiscalFixture>
    {
        private NotaFiscalFixture fixture;

        public EnviarNotaActorTest(NotaFiscalFixture fixture)
        {
            this.fixture = fixture;
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);
        }

        [Fact]
        public void Should_send_nota_to_sefaz_when_nota_is_valid()
        {
            var configuracaoRepositoryMock = new Mock<IConfiguracaoRepository>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var emiteNotaContingenciaServiceMock = new Mock<IEmiteNotaFiscalContingenciaFacade>();

            var subject = Props.Create(() => new EnviarNotaActor(
                configuracaoRepositoryMock.Object, serviceFactoryMock.Object, nfeConsultaMock.Object,
                emiteNotaContingenciaServiceMock.Object));
            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var message = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
        }
    }
}
