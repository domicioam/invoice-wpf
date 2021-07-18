using Akka.Actor;
using Akka.TestKit.Xunit2;
using AutoFixture;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using System;
using Xunit;
using static DgSystems.NFe.Core.UnitTests.Services.Actors.EmiteNFeContingenciaActorMock;

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
                            TipoMensagem.ErroValidacao,
                            ResultadoEsperado.Erro)));

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
                            TipoMensagem.ServicoIndisponivel,
                            ResultadoEsperado.Erro)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
        }

        [Fact]
        public void Deve_consultar_recibos_e_retornar_erro()
        {
            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            var emissorServiceMock = new Mock<IEmitenteRepository>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var certificadoServiceMock = new Mock<ICertificadoService>();
            var sefazSettingsMock = new Mock<SefazSettings>();

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(fixture.NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new global::NFe.Core.Entitities.NotaFiscalEntity());

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
                            TipoMensagem.Sucesso,
                            ResultadoEsperado.Erro)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 2), TimeSpan.FromSeconds(30));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 2), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void Deve_retornar_erro_duplicidade()
        {
            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            var emissorServiceMock = new Mock<IEmitenteRepository>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var certificadoServiceMock = new Mock<ICertificadoService>();
            var sefazSettingsMock = new Mock<SefazSettings>();

            nfeConsultaMock
                .Setup(n =>
                    n.ConsultarNotaFiscal(It.IsAny<string>(),
                    It.IsAny<string>(), null,
                    It.IsAny<global::NFe.Core.Domain.Modelo>()))
                .Returns(new MensagemRetornoConsulta());

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(fixture.NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new global::NFe.Core.Entitities.NotaFiscalEntity { Chave = "", Modelo = "65" });

            emissorServiceMock
                .Setup(e => e.GetEmissor())
                .Returns(new global::NFe.Core.Domain.Emissor { Endereco = new global::NFe.Core.Domain.Endereco() });

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
                            TipoMensagem.Sucesso,
                            ResultadoEsperado.Duplicidade)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 2), TimeSpan.FromSeconds(50));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 2), TimeSpan.FromSeconds(50));
        }

        [Fact]
        public void Deve_corrigir_nota_duplicada()
        {
            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            var emissorServiceMock = new Mock<IEmitenteRepository>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var certificadoServiceMock = new Mock<ICertificadoService>();
            var sefazSettingsMock = new Mock<SefazSettings>();

            nfeConsultaMock
                .Setup(n =>
                    n.ConsultarNotaFiscal(It.IsAny<string>(),
                    It.IsAny<string>(), null,
                    It.IsAny<global::NFe.Core.Domain.Modelo>()))
                .Returns(new MensagemRetornoConsulta() { IsEnviada = true });

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(fixture.NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new global::NFe.Core.Entitities.NotaFiscalEntity { Chave = "", Modelo = "65" });

            emissorServiceMock
                .Setup(e => e.GetEmissor())
                .Returns(new global::NFe.Core.Domain.Emissor { Endereco = new global::NFe.Core.Domain.Endereco() });

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
                            TipoMensagem.Sucesso,
                            ResultadoEsperado.Duplicidade)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 2));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 2));
        }

        [Fact]
        public void Deve_enviar_nota_quando_codigo_status_100()
        {
            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            var emissorServiceMock = new Mock<IEmitenteRepository>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var certificadoServiceMock = new Mock<ICertificadoService>();
            var sefazSettingsMock = new Mock<SefazSettings>();

            nfeConsultaMock
                .Setup(n =>
                    n.ConsultarNotaFiscal(It.IsAny<string>(),
                    It.IsAny<string>(), null,
                    It.IsAny<global::NFe.Core.Domain.Modelo>()))
                .Returns(new MensagemRetornoConsulta() { IsEnviada = true });

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(fixture.NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new global::NFe.Core.Entitities.NotaFiscalEntity { Chave = "", Modelo = "65" });

            emissorServiceMock
                .Setup(e => e.GetEmissor())
                .Returns(new global::NFe.Core.Domain.Emissor { Endereco = new global::NFe.Core.Domain.Endereco() });

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
                            TipoMensagem.Sucesso,
                            ResultadoEsperado.CodigoStatus100)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 0));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 0));
        }
    }
}
