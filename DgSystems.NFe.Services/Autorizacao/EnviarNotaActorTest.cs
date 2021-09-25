using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akka.Util;
using AutoMapper;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;
using Autorizacao = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using Consulta = NFe.Core.XmlSchemas.NfeConsulta2.Retorno;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EnviarNotaActorTest : TestKit, IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture fixture;
        Mock<IConfiguracaoRepository> configuracaoRepositoryMock = new Mock<IConfiguracaoRepository>();
        Mock<IServiceFactory> serviceFactoryMock = new Mock<IServiceFactory>();
        Mock<IConsultarNotaFiscalService> nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
        Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaContingenciaServiceMock = new Mock<IEmiteNotaFiscalContingenciaFacade>();
        Mock<NFeAutorizacao4Soap> nfeAutorizacaoSoapMock = new Mock<NFeAutorizacao4Soap>();
        Mock<IMapper> mapper = new Mock<IMapper>();

        public EnviarNotaActorTest(NotaFiscalFixture fixture)
        {
            this.fixture = fixture;
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);
        }

        [Fact]
        public void Should_send_nota_to_sefaz_when_nota_is_valid()
        {
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s =>
                s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var message = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(
                configuracaoRepositoryMock.Object, serviceFactoryMock.Object, nfeConsultaMock.Object,
                emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));
            subject.Tell(message);
            ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void Should_emitir_em_contingencia_quando_contingencia_ativada()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity() { IsContingencia = true}));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var message = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(message);

            // Then
            ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10));
            emiteNotaContingenciaServiceMock.Verify(e => e.SaveNotaFiscalContingenciaAsync(It.IsAny<X509Certificate2>(),
                It.IsAny<ConfiguracaoEntity>(), It.IsAny<NotaFiscal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public void Should_retornar_sucesso_apos_resposta_sefaz_actor_success()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            mapper.Setup(m => m.Map<Autorizacao.TProtNFe>(It.IsAny<Consulta.TProtNFe>())).Returns(fixture.ProtNFe);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = new Result<TProtNFe>(fixture.ProtNFe);
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            var resultadoEnvio = ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10)).Status as ResultadoEnvio;
            Assert.NotNull(resultadoEnvio);
            Assert.Equal(fixture.ProtNFe.infProt.nProt, resultadoEnvio.NotaFiscal.ProtocoloAutorizacao);
            Assert.Equal(DateTime.ParseExact(fixture.ProtNFe.infProt.dhRecbto, NotaFiscalFixture.DATE_STRING_SEFAZ_FORMAT, CultureInfo.InvariantCulture), resultadoEnvio.NotaFiscal.DataHoraAutorização);
            Assert.Equal(global::NFe.Core.Entitities.Status.ENVIADA, resultadoEnvio.NotaFiscal.Identificacao.Status.Status);
        }


        [Fact]
        public void Should_fix_nota_when_sent_but_no_success_response()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            mapper.Setup(m => m.Map<Autorizacao.TProtNFe>(It.IsAny<Consulta.TProtNFe>())).Returns(fixture.ProtNFe);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Throws(new Exception());

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            nfeConsultaMock.Setup(n => n.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta { Protocolo = new Protocolo(fixture.ProtNFeConsulta), IsEnviada = true });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = Result.Failure<TProtNFe>(new Exception());
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            var resultadoEnvio = ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10)).Status as ResultadoEnvio;
            Assert.NotNull(resultadoEnvio);
            Assert.Equal(fixture.ProtNFe.infProt.nProt, resultadoEnvio.NotaFiscal.ProtocoloAutorizacao);
            Assert.Equal(DateTime.ParseExact(fixture.ProtNFe.infProt.dhRecbto, NotaFiscalFixture.DATE_STRING_SEFAZ_FORMAT, CultureInfo.InvariantCulture), resultadoEnvio.NotaFiscal.DataHoraAutorização);
            Assert.Equal(global::NFe.Core.Entitities.Status.ENVIADA, resultadoEnvio.NotaFiscal.Identificacao.Status.Status);
        }

        [Fact]
        public void Should_reply_failure_when_nota_not_sent()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            mapper.Setup(m => m.Map<Autorizacao.TProtNFe>(It.IsAny<Consulta.TProtNFe>())).Returns(fixture.ProtNFe);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Throws(new Exception());

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            nfeConsultaMock.Setup(n => n.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta { IsEnviada = false });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = Result.Failure<TProtNFe>(new Exception());
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            ExpectMsg<Status.Failure>(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void Should_fix_nota_duplicada()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            mapper.Setup(m => m.Map<Autorizacao.TProtNFe>(It.IsAny<Consulta.TProtNFe>())).Returns(fixture.ProtNFe);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            nfeConsultaMock.Setup(n => n.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta { Protocolo = new Protocolo(fixture.ProtNFeConsulta) });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = new Result<TProtNFe>(fixture.ProtNFeMotivoDuplicada);
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            var resultadoEnvio = ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10)).Status as ResultadoEnvio;
            Assert.NotNull(resultadoEnvio);
            Assert.Equal(fixture.ProtNFe.infProt.nProt, resultadoEnvio.NotaFiscal.ProtocoloAutorizacao);
            Assert.Equal(DateTime.ParseExact(fixture.ProtNFe.infProt.dhRecbto, NotaFiscalFixture.DATE_STRING_SEFAZ_FORMAT, CultureInfo.InvariantCulture), resultadoEnvio.NotaFiscal.DataHoraAutorização);
            Assert.Equal(global::NFe.Core.Entitities.Status.ENVIADA, resultadoEnvio.NotaFiscal.Identificacao.Status.Status);
        }

        [Fact]
        public void Should_fix_nota_fiscal_already_sent()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            mapper.Setup(m => m.Map<Autorizacao.TProtNFe>(It.IsAny<Consulta.TProtNFe>())).Returns(fixture.ProtNFe);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            nfeConsultaMock.Setup(n => n.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta { Protocolo = new Protocolo(fixture.ProtNFeConsulta), IsEnviada = true });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = new Result<Exception>(new Exception());
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            var resultadoEnvio = ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10)).Status as ResultadoEnvio;
            Assert.NotNull(resultadoEnvio);
            Assert.Equal(fixture.ProtNFe.infProt.nProt, resultadoEnvio.NotaFiscal.ProtocoloAutorizacao);
            Assert.Equal(DateTime.ParseExact(fixture.ProtNFe.infProt.dhRecbto, NotaFiscalFixture.DATE_STRING_SEFAZ_FORMAT, CultureInfo.InvariantCulture), resultadoEnvio.NotaFiscal.DataHoraAutorização);
            Assert.Equal(global::NFe.Core.Entitities.Status.ENVIADA, resultadoEnvio.NotaFiscal.Identificacao.Status.Status);
        }

        [Fact]
        public void Should_return_failure_when_nota_fiscal_not_sent()
        {
            // Given
            var contingenciaActor = CreateTestProbe();
            var modoOnlineActor = CreateTestProbe();

            mapper.Setup(m => m.Map<Autorizacao.TProtNFe>(It.IsAny<Consulta.TProtNFe>())).Returns(fixture.ProtNFe);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Throws(new Exception());

            nfeConsultaMock.Setup(n => n.ConsultarNotaFiscal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<Modelo>()))
                .Returns(new RetornoConsulta { IsEnviada = false });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = Result.Failure<Exception>(new Exception());
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object,
                nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor.Ref)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            ExpectMsg<Status.Success>(TimeSpan.FromSeconds(40));
        }
    }
}
