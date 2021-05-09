using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.Domain;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using NFe.Core.UnitTests.NotaFiscalService;
using NFe.Core.Utils.Assinatura;
using Xunit;
using NFe.Core.NotasFiscais;
using DgSystems.NFe.Core.Cadastro;
using NFe.Core.Cadastro.Ibpt;

namespace DgSystems.NFe.Core.UnitTests.NotaFiscalService
{
    public class NotaFiscalServiceTest : IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture _fixture;

        public NotaFiscalServiceTest(NotaFiscalFixture fixture)
        {
            _fixture = fixture;
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);
        }
        
        [Fact]
        public void EnviarNotaFiscalAsync_ContingênciaAtivada_NãoCriaNotaPendente()
        {
            // Arrange

            ConfigurarCertificadoDigital(out var certificadoRepositoryMock, out var certificadoManagerMock);

            var serviceFactoryMock = ConfigurarServiceFactoryMock();

            ConfigurarConfiguracao();

            var configuracaoRepository = new ConfiguracaoRepositoryFake();

            var certificadoRepository = certificadoRepositoryMock.Object;
            var serviceFactory = serviceFactoryMock.Object;
            var nfeConsulta = new Mock<IConsultarNotaFiscalService>().Object;
            var certificateManager = certificadoManagerMock.Object;

            var consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>().Object;

            var emissorService = new Mock<IEmitenteRepository>().Object;
            var certificadoService = new Mock<ICertificadoService>().Object;
            var notaInutilizadaFacade = new Mock<InutilizarNotaFiscalService>().Object;
            var cancelaNotaFiscalService = new Mock<ICancelaNotaFiscalService>().Object;

            INotaFiscalRepository notaFiscalRepository = new NotaFiscalRepositoryFake();
            var notaFiscalContingenciaService = new EmiteNotaFiscalContingenciaFacade(configuracaoRepository, notaFiscalRepository, emissorService,
                nfeConsulta, serviceFactory, certificadoService, notaInutilizadaFacade, cancelaNotaFiscalService, new global::NFe.Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao });

            var notaFiscalService = new EnviarNotaFiscalService(configuracaoRepository, serviceFactory, nfeConsulta);

            var modoOnlineService = new ModoOnlineService(configuracaoRepository, consultaStatusServicoService, notaFiscalRepository, notaFiscalContingenciaService);

            // preencher nota fiscal;
            NotaFiscal notaFiscal = PreencherNotaFiscal();

            // Act

            var xmlNfe = new XmlNFe(notaFiscal, _fixture.NfeNamespaceName, _fixture.X509Certificate2, _fixture.CscId,
                _fixture.Csc);
            
            notaFiscalService.EnviarNotaFiscal(notaFiscal, _fixture.CscId, _fixture.Csc, _fixture.X509Certificate2, xmlNfe);
            notaFiscalService.EnviarNotaFiscal(notaFiscal, _fixture.CscId, _fixture.Csc, _fixture.X509Certificate2, xmlNfe);

            // Assert

            var isContingência = configuracaoRepository.GetConfiguracao().IsContingencia;
            var quantidadeNotasPendentes = notaFiscalRepository.GetAll().Count(n => n.Status == (int)Status.PENDENTE);
            var quantidadeTotalNotas = notaFiscalRepository.GetAll().Count();

            Assert.True(isContingência);
            Assert.Equal(3, quantidadeTotalNotas);
            Assert.Equal(1, quantidadeNotasPendentes);
        }

        [Fact]
        public void EnviarNotaFiscalAsync_FalhaDeConexão_AtivaContingencia()
        {
            ConfigurarCertificadoDigital(out var certificadoRepositoryMock, out var certificadoManagerMock);

            var serviceFactoryMock = ConfigurarServiceFactoryMock();

            ConfigurarConfiguracao();
            var configuracaoRepository = new ConfiguracaoRepositoryFake();

            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            notaFiscalRepositoryMock
                .Setup(m => m.GetNotaFiscalById(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new NotaFiscalEntity());

            var notaFiscalRepository = notaFiscalRepositoryMock.Object;
            var serviceFactory = serviceFactoryMock.Object;
            var nfeConsulta = new Mock<IConsultarNotaFiscalService>().Object;
            var notaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>().Object;

            var consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>().Object;

            var notaFiscalService = new EnviarNotaFiscalService(configuracaoRepository, serviceFactory, nfeConsulta);
            var modoOnlineService = new ModoOnlineService(
                configuracaoRepository, consultaStatusServicoService, notaFiscalRepository,
                notaFiscalContingenciaService);

            // preencher nota fiscal;
            var notaFiscal = PreencherNotaFiscal();

            // envia com falha de conexão no serviço;
            var xmlNfe = new XmlNFe(notaFiscal, _fixture.NfeNamespaceName, _fixture.X509Certificate2, _fixture.CscId,
                _fixture.Csc);

            notaFiscalService.EnviarNotaFiscal(notaFiscal, _fixture.CscId, _fixture.Csc, _fixture.X509Certificate2, xmlNfe);

            // verifica se modo contingência foi ativado;
            var isContingência = configuracaoRepository.GetConfiguracao().IsContingencia;
            Assert.True(isContingência);
        }

        [Fact]
        public void EnviarNotaFiscalAsync_FalhaDeConexão_AtivaContingência_CriaNotaPendente()
        {
            // Arrange

            Mock<ICertificadoRepository> certificadoRepositoryMock;
            Mock<ICertificadoService> certificadoManagerMock;
            ConfigurarCertificadoDigital(out certificadoRepositoryMock, out certificadoManagerMock);

            var serviceFactoryMock = ConfigurarServiceFactoryMock();

            ConfigurarConfiguracao();

            var configuracaoRepository = new ConfiguracaoRepositoryFake();

            INotaFiscalRepository notaFiscalRepository = new NotaFiscalRepositoryFake();
            var certificadoRepository = certificadoRepositoryMock.Object;
            var serviceFactory = serviceFactoryMock.Object;
            var nfeConsulta = new Mock<IConsultarNotaFiscalService>().Object;
            var certificateManager = certificadoManagerMock.Object;
            var consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>().Object;

            var emissorService = new Mock<IEmitenteRepository>().Object;
            var certificadoService = new Mock<ICertificadoService>().Object;
            var notaInutilizadaFacade = new Mock<InutilizarNotaFiscalService>().Object;
            var cancelaNotaFiscalService = new Mock<ICancelaNotaFiscalService>().Object;

            var notaFiscalContingenciaService = new EmiteNotaFiscalContingenciaFacade(configuracaoRepository, notaFiscalRepository, emissorService, nfeConsulta, serviceFactory, certificadoService, notaInutilizadaFacade, cancelaNotaFiscalService, new global::NFe.Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao });

            var notaFiscalService = new EnviarNotaFiscalService(configuracaoRepository, serviceFactory, nfeConsulta);

            var modoOnlineService = new ModoOnlineService(
                configuracaoRepository, consultaStatusServicoService, notaFiscalRepository,
                notaFiscalContingenciaService);

            // preencher nota fiscal;
            NotaFiscal notaFiscal = PreencherNotaFiscal();

            // Act

            var xmlNfe = new XmlNFe(notaFiscal, _fixture.NfeNamespaceName, _fixture.X509Certificate2, _fixture.CscId,
                _fixture.Csc);

            notaFiscalService.EnviarNotaFiscal(notaFiscal, _fixture.CscId, _fixture.Csc, _fixture.X509Certificate2, xmlNfe);

            // Assert

            var isContingência = configuracaoRepository.GetConfiguracao().IsContingencia;
            var quantidadeNotasPendentes = notaFiscalRepository.GetAll().Count(n => n.Status == (int)Status.PENDENTE);
            var quantidadeTotalNotas = notaFiscalRepository.GetAll().Count();

            Assert.True(isContingência);
            Assert.Equal(1, quantidadeNotasPendentes);
            Assert.Equal(2, quantidadeTotalNotas);
        }

        [Fact]
        public void NotaFiscalService_EnviarNotaFiscal_Sucesso()
        {

        }

        private static void ConfigurarConfiguracao()
        {
            var configuracao = new ConfiguracaoEntity
            {
                Id = 1,
                SerieNFCe = "001",
                SerieNFe = "001",
                ProximoNumNFCe = "1",
                ProximoNumNFe = "1",
            };

            var configuracaoServiceMock = new Mock<IConfiguracaoRepository>();
            configuracaoServiceMock.Setup(m => m.GetConfiguracao()).Returns(configuracao);
        }

        private static Mock<IServiceFactory> ConfigurarServiceFactoryMock()
        {
            var nfeAutorizacao4Mock = new Mock<NFeAutorizacao4Soap>();
            nfeAutorizacao4Mock
                .Setup(m => m.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Throws(new WebException());

            var serviceFactoryMock = new Mock<IServiceFactory>();
            serviceFactoryMock.Setup(m => m.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                              .Returns(() => new Service { SoapClient = nfeAutorizacao4Mock.Object });
            return serviceFactoryMock;
        }

        private void ConfigurarCertificadoDigital(out Mock<ICertificadoRepository> certificadoRepositoryMock, out Mock<ICertificadoService> certificadoManagerMock)
        {
            certificadoRepositoryMock = new Mock<ICertificadoRepository>();
            certificadoRepositoryMock
                .Setup(m => m.GetCertificado())
                .Returns(() => _fixture.CertificadoEntity );

            certificadoManagerMock = new Mock<ICertificadoService>();
            certificadoManagerMock
                .Setup(m => m.GetX509Certificate2())
                .Returns(() => _fixture.X509Certificate2);
        }

        private static NotaFiscal PreencherNotaFiscal()
        {
            var endereçoEmitente = new Endereco("QUADRA 200 CONJUNTO 20", "20", "BRASILIA", "BRASILIA", "70000000", "DF");
            var emitente = new Emissor("RAZAO SOCIAL", "NOME FANTASIA", "12345678998765", "1234567898765",
                "1234567898765", "4784900", "Regime Normal", endereçoEmitente, "99999999");
            var identificação = new IdentificacaoNFe(CodigoUfIbge.DF, DateTime.Now, emitente.CNPJ, Modelo.Modelo65, 1,
                "20887", TipoEmissao.Normal, Ambiente.Homologacao, emitente, "Venda", FinalidadeEmissao.Normal, true,
                PresencaComprador.Presencial, "CPF");
            var transporte = new Transporte(Modelo.Modelo65, null, null);
            const int valorTotalProdutos = 65;
            var totalIcms = new IcmsTotal(0, 0, 0, 0, 0, valorTotalProdutos, 0, 0, 0, 0, 0, 0, 0, 0, valorTotalProdutos,
                0);
            var impostosList = new List<global::NFe.Core.Domain.Imposto>
            {
                new global::NFe.Core.Domain.Imposto {CST = "60", TipoImposto = TipoImposto.Icms},
                new global::NFe.Core.Domain.Imposto {CST = "04", TipoImposto = TipoImposto.PIS}
            };

            var impostos = new Impostos(impostosList);

            var produtos = new List<Produto>
            {
                new Produto(impostos, 0, "5656", "0001", "GLP 13KG", "27111910", 1, "UN", 65, 0, false,0,0,0)
            };
            var pagamentos = new List<Pagamento>
            {
                new Pagamento(FormaPagamento.Dinheiro) {Valor = 65}
            };
            var infoAdicional = new InfoAdicional(produtos, new IbptManager());
            var notaFiscal = new NotaFiscal(emitente, null, identificação, transporte, totalIcms,null,null, infoAdicional,
                produtos, pagamentos);
            return notaFiscal;
        }
    }
}