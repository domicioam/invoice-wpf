using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.TestKit.Xunit2;
using Xunit;
using Moq;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz;
using Akka.Actor;
using DgSystems.NFe.Services.Actors;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;

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
            configuracaoRepository.Verify(c => c.GetConfiguracao(), Times.Once);
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
            consultaStatusServicoService.Setup(c => c.ExecutarConsultaStatus(It.IsAny<ConfiguracaoEntity>(), It.IsAny<Modelo>())).Returns(true);

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
            configuracaoRepository.Verify(c => c.GetConfiguracao(), Times.Exactly(2));
        }
    }
}
