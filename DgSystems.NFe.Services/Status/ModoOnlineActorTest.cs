using Akka.Actor;
using Akka.TestKit.Xunit2;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests
{
    public class ModoOnlineActorTest : TestKit
    {
        [Fact]
        public void Should_activate_modo_online()
        {
            // Given
            Mock<IConfiguracaoRepository> configuracaoRepository = new Mock<IConfiguracaoRepository>();
            configuracaoRepository.Setup(c => c.GetConfiguracao()).Returns(new ConfiguracaoEntity());
            Mock<IConsultaStatusServicoSefazService> consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>();
            Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
            Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            Mock<IEmitenteRepository> emissorService = new Mock<IEmitenteRepository>();
            Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();

            var contingenciaActor = CreateTestProbe();

            var subject = Sys.ActorOf(Props.Create(() => new ModoOnlineActor(
                configuracaoRepository.Object, consultaStatusServicoService.Object, notaFiscalRepository.Object,
                emiteNotaFiscalContingenciaService.Object, emissorService.Object, nfeConsulta.Object,
                serviceFactory.Object, certificadoService.Object, sefazSettings.Object, _ => contingenciaActor.Ref)));

            // When
            subject.Tell(new ModoOnlineActor.AtivarModoOnline());

            // Then
            contingenciaActor.ExpectMsg<EmiteNFeContingenciaActor.TransmitirNFeEmContingencia>();
            configuracaoRepository.Verify(c => c.Salvar(It.Is<ConfiguracaoEntity>(config => config.IsContingencia == false)), Times.Once);
        }

        [Fact]
        public void Should_activate_modo_online_when_started_and_service_is_up()
        {
            // Given
            Mock<IConfiguracaoRepository> configuracaoRepository = new Mock<IConfiguracaoRepository>();
            Mock<IConsultaStatusServicoSefazService> consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>();
            Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
            Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            Mock<IEmitenteRepository> emissorService = new Mock<IEmitenteRepository>();
            Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();

            configuracaoRepository.Setup(c => c.GetConfiguracao()).Returns(new ConfiguracaoEntity());
            consultaStatusServicoService.Setup(c => c.ExecutarConsultaStatus(It.IsAny<Modelo>(), ConfigurationManager.AppSettings["sefazEnvironment"])).Returns(true);

            var contingenciaActor = CreateTestProbe();

            var subject = Sys.ActorOf(Props.Create(() => new ModoOnlineActor(
                configuracaoRepository.Object, consultaStatusServicoService.Object, notaFiscalRepository.Object,
                emiteNotaFiscalContingenciaService.Object, emissorService.Object, nfeConsulta.Object,
                serviceFactory.Object, certificadoService.Object, sefazSettings.Object, _ => contingenciaActor.Ref)));

            // When
            subject.Tell(new ModoOnlineActor.Start());

            // Then
            contingenciaActor.ExpectMsg<EmiteNFeContingenciaActor.TransmitirNFeEmContingencia>();
            configuracaoRepository.Verify(c => c.Salvar(It.Is<ConfiguracaoEntity>(config => config.IsContingencia == false)), Times.Once);
        }

        [Fact]
        public void Should_activate_modo_offline_when_started_and_service_is_down()
        {
            // Given
            Mock<IConfiguracaoRepository> configuracaoRepository = new Mock<IConfiguracaoRepository>();
            Mock<IConsultaStatusServicoSefazService> consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>();
            Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
            Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            Mock<IEmitenteRepository> emissorService = new Mock<IEmitenteRepository>();
            Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();

            configuracaoRepository.Setup(c => c.GetConfiguracao()).Returns(new ConfiguracaoEntity() { IsContingencia = false, JustificativaContingencia = null });
            consultaStatusServicoService.Setup(c => c.ExecutarConsultaStatus(It.IsAny<Modelo>(), ConfigurationManager.AppSettings["sefazEnvironment"])).Returns(false);
            int count = 0;
            MessagingCenter.Subscribe<ModoOnlineActor, ServicoOfflineEvent>(this, nameof(ServicoOfflineEvent), (s, e) => count++);

            var contingenciaActor = CreateTestProbe();

            var subject = Sys.ActorOf(Props.Create(() => new ModoOnlineActor(
                configuracaoRepository.Object, consultaStatusServicoService.Object, notaFiscalRepository.Object,
                emiteNotaFiscalContingenciaService.Object, emissorService.Object, nfeConsulta.Object,
                serviceFactory.Object, certificadoService.Object, sefazSettings.Object, _ => contingenciaActor.Ref)));

            // When
            subject.Tell(new ModoOnlineActor.Start());

            // Then
            contingenciaActor.ExpectNoMsg();
            configuracaoRepository.Verify(c => c.Salvar(It.Is<ConfiguracaoEntity>(config => config.IsContingencia && !string.IsNullOrEmpty(config.JustificativaContingencia))), Times.Once);
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            Assert.Equal(1, count);
        }

        [Fact]
        public void Should_inutilizar_ou_cancelar_notas_pendentes_contingencia()
        {
            // Given
            Mock<IConfiguracaoRepository> configuracaoRepository = new Mock<IConfiguracaoRepository>();
            Mock<IConsultaStatusServicoSefazService> consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>();
            Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
            Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            Mock<IEmitenteRepository> emissorService = new Mock<IEmitenteRepository>();
            Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();

            notaFiscalRepository
                .Setup(n => n.GetPrimeiraNotaEmitidaEmContingencia(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new NotaFiscalEntity() { Numero = "1" });

            notaFiscalRepository
                .Setup(n => n.GetNota(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new NotaFiscalEntity());
            
            configuracaoRepository.Setup(c => c.GetConfiguracao()).Returns(new ConfiguracaoEntity() { IsContingencia = false, JustificativaContingencia = null });

            int count = 0;
            MessagingCenter.Subscribe<ModoOnlineActor, NotasFiscaisTransmitidasEvent>(this, nameof(NotasFiscaisTransmitidasEvent), (s, e) => count++);

            var contingenciaActor = CreateTestProbe();

            var subject = Sys.ActorOf(Props.Create(() => new ModoOnlineActor(
                configuracaoRepository.Object, consultaStatusServicoService.Object, notaFiscalRepository.Object,
                emiteNotaFiscalContingenciaService.Object, emissorService.Object, nfeConsulta.Object,
                serviceFactory.Object, certificadoService.Object, sefazSettings.Object, _ => contingenciaActor.Ref)));

            // When 
            subject.Tell(new EmiteNFeContingenciaActor.ResultadoNotasTransmitidas(new List<string>()));

            // Then
            contingenciaActor.ExpectNoMsg();
            emiteNotaFiscalContingenciaService
                .Verify(e => e.InutilizarCancelarNotasPendentesContingencia(
                    It.IsAny<NotaFiscalEntity>(), It.IsAny<INotaFiscalRepository>()), Times.Once);
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            Assert.Equal(1, count);
        }

        [Fact]
        public void Should_send_event_when_exception_thrown()
        {
            // Given
            Mock<IConfiguracaoRepository> configuracaoRepository = new Mock<IConfiguracaoRepository>();
            Mock<IConsultaStatusServicoSefazService> consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>();
            Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
            Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            Mock<IEmitenteRepository> emissorService = new Mock<IEmitenteRepository>();
            Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();

            int count = 0;
            MessagingCenter.Subscribe<ModoOnlineActor, NotasFiscaisTransmitidasEvent>(this, nameof(NotasFiscaisTransmitidasEvent), (s, e) => count++);

            var contingenciaActor = CreateTestProbe();

            var subject = Sys.ActorOf(Props.Create(() => new ModoOnlineActor(
                configuracaoRepository.Object, consultaStatusServicoService.Object, notaFiscalRepository.Object,
                emiteNotaFiscalContingenciaService.Object, emissorService.Object, nfeConsulta.Object,
                serviceFactory.Object, certificadoService.Object, sefazSettings.Object, _ => contingenciaActor.Ref)));

            // When 
            subject.Tell(new EmiteNFeContingenciaActor.ResultadoNotasTransmitidas(new List<string>()));

            // Then
            contingenciaActor.ExpectNoMsg();
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            Assert.Equal(1, count);
        }
    }
}
