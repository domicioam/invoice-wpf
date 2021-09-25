using Akka.Actor;
using Akka.TestKit.Xunit2;
using AutoMapper;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using System;
using System.Collections.Generic;
using Xunit;
using static DgSystems.NFe.Core.UnitTests.Services.Actors.EmiteNFeContingenciaActorMock;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EmiteNFeContingenciaActorTest : TestKit, IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture fixture;

        public Mock<INotaFiscalRepository> notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
        public Mock<IEmitenteRepository> emissorServiceMock = new Mock<IEmitenteRepository>();
        public Mock<IConsultarNotaFiscalService> nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
        public Mock<IServiceFactory> serviceFactoryMock = new Mock<IServiceFactory>();
        public Mock<CertificadoService> certificadoServiceMock = new Mock<CertificadoService>();
        public Mock<SefazSettings> sefazSettingsMock = new Mock<SefazSettings>();
        public Mock<IMapper> mapperMock = new Mock<IMapper>();

        public List<NotaFiscalEntity> NotasContingencia =>
            new List<NotaFiscalEntity> {
                new NotaFiscalEntity { Modelo = "55" },
                new NotaFiscalEntity { Modelo = "65" }
        };

        public EmiteNFeContingenciaActorTest(NotaFiscalFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Deve_responder_erro_quando_validacao_falha()
        {
            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(NotasContingencia);

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
                            ResultadoEsperado.Erro,
                            mapperMock.Object)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
        }

        [Fact]
        public void Deve_responder_erro_quando_servico_indisponivel()
        {
            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(NotasContingencia);

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
                            ResultadoEsperado.Erro,
                            mapperMock.Object)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count != 0));
        }

        [Fact]
        public void Deve_consultar_recibos_e_retornar_erro()
        {
            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity());

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
                            ResultadoEsperado.Erro,
                            mapperMock.Object)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.Equal(2, msg.Erros.Count), TimeSpan.FromSeconds(30));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.Equal(2, msg.Erros.Count), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void Deve_retornar_erro_duplicidade()
        {
            nfeConsultaMock
                .Setup(n =>
                    n.ConsultarNotaFiscal(It.IsAny<string>(),
                    It.IsAny<string>(), null,
                    It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta());

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Chave = "", Modelo = "65" });

            emissorServiceMock
                .Setup(e => e.GetEmissor())
                .Returns(new Emissor { Endereco = new Endereco() });

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
                            ResultadoEsperado.Duplicidade,
                            mapperMock.Object)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.Equal(2, msg.Erros.Count), TimeSpan.FromSeconds(50));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.Equal(2, msg.Erros.Count), TimeSpan.FromSeconds(50));
        }

        [Fact]
        public void Deve_corrigir_nota_duplicada()
        {
            nfeConsultaMock
                .Setup(n =>
                    n.ConsultarNotaFiscal(It.IsAny<string>(),
                    It.IsAny<string>(), null,
                    It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta() { IsEnviada = true, Protocolo = new Protocolo(fixture.ProtNFeConsulta) });

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Chave = "", Modelo = "65" });

            emissorServiceMock
                .Setup(e => e.GetEmissor())
                .Returns(new Emissor { Endereco = new Endereco() });

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
                            ResultadoEsperado.CorrigeDuplicidade,
                            mapperMock.Object)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 0), TimeSpan.FromSeconds(50));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 0), TimeSpan.FromSeconds(50));
            Assert.Equal(4, TentaCorrigirNotaDuplicadaCount);
            notaFiscalRepositoryMock.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), It.IsAny<string>()), Times.Exactly(4));
        }

        [Fact]
        public void Deve_enviar_nota_quando_codigo_status_100()
        {
            nfeConsultaMock
                .Setup(n =>
                    n.ConsultarNotaFiscal(It.IsAny<string>(),
                    It.IsAny<string>(), null,
                    It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta() { IsEnviada = true });

            notaFiscalRepositoryMock.Setup(n => n.GetNotasContingencia()).Returns(NotasContingencia);
            notaFiscalRepositoryMock
                .Setup(n => n.GetNotaFiscalByChave(It.IsAny<string>()))
                .Returns(new NotaFiscalEntity { Chave = "", Modelo = "65" });

            emissorServiceMock
                .Setup(e => e.GetEmissor())
                .Returns(new Emissor { Endereco = new Endereco() });

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
                            ResultadoEsperado.CodigoStatus100,
                            mapperMock.Object)));

            subject.Tell(new EmiteNFeContingenciaActor.TransmitirNFeEmContingencia());

            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 0), TimeSpan.FromSeconds(50));
            ExpectMsg<EmiteNFeContingenciaActor.ResultadoNotasTransmitidas>(msg => Assert.True(msg.Erros.Count == 0), TimeSpan.FromSeconds(50));
            notaFiscalRepositoryMock.Verify(n => n.Salvar(It.IsAny<NotaFiscalEntity>(), It.IsAny<string>()));
            Assert.Equal(4, PreencheDadosNotaFiscalAposEnvioCount);
        }
    }
}