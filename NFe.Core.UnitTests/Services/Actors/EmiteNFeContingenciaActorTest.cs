using Akka.Actor;
using Akka.TestKit.Xunit2;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using Xunit;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EmiteNFeContingenciaActorTest : TestKit, IClassFixture<EmiteNFeContingenciaFixture>
    {
        private readonly EmiteNFeContingenciaFixture fixture;

        public EmiteNFeContingenciaActorTest(EmiteNFeContingenciaFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Deve_responder_erro_quando_validacao_falha()
        {
            // HandleTransmiteNFes
            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            var emissorServiceMock = new Mock<IEmitenteRepository>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var certificadoServiceMock = new Mock<ICertificadoService>();
            var sefazSettingsMock = new Mock<SefazSettings>();

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(fixture.NotasContingencia);

            var subject = 
                Sys.ActorOf(
                    Props.Create(() => 
                        new EmiteNFeContingenciaActorMock(
                            notaFiscalRepositoryMock.Object,
                            emissorServiceMock.Object, 
                            nfeConsultaMock.Object, 
                            serviceFactoryMock.Object, 
                            certificadoServiceMock.Object,
                            sefazSettingsMock.Object,
                            TipoMensagem.ErroValidacao)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0)); 
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0)); 
        }

        [Fact]
        public void Deve_responder_erro_quando_servico_indisponivel()
        {
            // HandleTransmiteNFes
            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            var emissorServiceMock = new Mock<IEmitenteRepository>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var certificadoServiceMock = new Mock<ICertificadoService>();
            var sefazSettingsMock = new Mock<SefazSettings>();

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(fixture.NotasContingencia);

            var subject =
                Sys.ActorOf(
                    Props.Create(() =>
                        new EmiteNFeContingenciaActorMock(
                            notaFiscalRepositoryMock.Object,
                            emissorServiceMock.Object,
                            nfeConsultaMock.Object,
                            serviceFactoryMock.Object,
                            certificadoServiceMock.Object,
                            sefazSettingsMock.Object,
                            TipoMensagem.ServicoIndisponivel)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
        }
    }
}
