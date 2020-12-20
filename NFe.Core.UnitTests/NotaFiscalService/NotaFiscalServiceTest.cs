using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Assinatura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.NotasFiscais.ValueObjects;
using Xunit;

namespace NFe.Core.UnitTests.NotaFiscalService
{
    public class NotaFiscalServiceTest
    {
        [Fact]
        public void EnviarNotaFiscalAsync_ContingênciaAtivada_NãoCriaNotaPendente()
        {
            // Arrange

            Mock<ICertificadoRepository> certificadoRepositoryMock;
            Mock<ICertificateManager> certificadoManagerMock;
            ConfigurarCertificadoDigital(out certificadoRepositoryMock, out certificadoManagerMock);

            Mock<IServiceFactory> serviceFactoryMock = ConfigurarServiceFactoryMock();

            ConfigurarConfiguracao();

            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var certificadoRepository = certificadoRepositoryMock.Object;
            var serviceFactory = serviceFactoryMock.Object;
            var nfeConsulta = new Mock<INFeConsulta>().Object;
            var certificateManager = certificadoManagerMock.Object;

            var consultaStatusServicoService = new Mock<IConsultaStatusServicoFacade>().Object;

            var emissorService = new Mock<IEmissorService>().Object;
            var certificadoService = new Mock<ICertificadoService>().Object;
            var notaInutilizadaFacade = new Mock<InutilizarNotaFiscalFacade>().Object;
            var cancelaNotaFiscalService = new Mock<ICancelaNotaFiscalFacade>().Object;

            INotaFiscalRepository notaFiscalRepository = new NotaFiscalRepositoryFake();
            var notaFiscalContingenciaService = new EmiteEmiteNotaFiscalContingenciaFacade(configuracaoService, certificadoRepository, certificateManager, notaFiscalRepository, emissorService,
                nfeConsulta, serviceFactory, certificadoService, notaInutilizadaFacade, cancelaNotaFiscalService, new Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao }, new Utils.RijndaelManagedEncryption());

            var notaFiscalService = new EnviarNotaFiscalService(configuracaoRepository, notaFiscalRepository,
                certificadoRepository, configuracaoService, serviceFactory, nfeConsulta, certificateManager,
                notaFiscalContingenciaService, new Utils.RijndaelManagedEncryption());

            var modoOnlineService = new NotasFiscais.Services.ModoOnlineService(
                configuracaoRepository, consultaStatusServicoService, notaFiscalRepository,
                notaFiscalContingenciaService);

            // preencher nota fiscal;
            NotaFiscal notaFiscal = PreencherNotaFiscal();

            // Act

            notaFiscalService.EnviarNotaFiscal(notaFiscal, "000001", "E3BB2129-7ED0-31A10-CCB8-1B8BAC8FA2D0");
            notaFiscalService.EnviarNotaFiscal(notaFiscal, "000001", "E3BB2129-7ED0-31A10-CCB8-1B8BAC8FA2D0");

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
            Mock<ICertificadoRepository> certificadoRepositoryMock;
            Mock<ICertificateManager> certificadoManagerMock;
            ConfigurarCertificadoDigital(out certificadoRepositoryMock, out certificadoManagerMock);

            Mock<IServiceFactory> serviceFactoryMock = ConfigurarServiceFactoryMock();

            ConfigurarConfiguracao();

            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
            notaFiscalRepositoryMock
                .Setup(m => m.GetNotaFiscalById(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new NotaFiscalEntity());

            var notaFiscalRepository = notaFiscalRepositoryMock.Object;
            var certificadoRepository = certificadoRepositoryMock.Object;
            var serviceFactory = serviceFactoryMock.Object;
            var nfeConsulta = new Mock<INFeConsulta>().Object;
            var certificateManager = certificadoManagerMock.Object;
            var notaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>().Object;

            var consultaStatusServicoService = new Mock<IConsultaStatusServicoFacade>().Object;

            var notaFiscalService = new EnviarNotaFiscalService(configuracaoRepository, notaFiscalRepository,
                certificadoRepository, configuracaoService, serviceFactory, nfeConsulta, certificateManager,
                notaFiscalContingenciaService, new Utils.RijndaelManagedEncryption());
            var modoOnlineService = new NotasFiscais.Services.ModoOnlineService(
                configuracaoRepository, consultaStatusServicoService, notaFiscalRepository,
                notaFiscalContingenciaService);

            // preencher nota fiscal;
            NotaFiscal notaFiscal = PreencherNotaFiscal();

            // envia com falha de conexão no serviço;
            notaFiscalService.EnviarNotaFiscal(notaFiscal, "000001", "E3BB2129-7ED0-31A10-CCB8-1B8BAC8FA2D0");
            // verifica se modo contingência foi ativado;
            var isContingência = configuracaoRepository.GetConfiguracao().IsContingencia;
            Assert.True(isContingência);
        }

        [Fact]
        public void EnviarNotaFiscalAsync_FalhaDeConexão_AtivaContingência_CriaNotaPendente()
        {
            // Arrange

            Mock<ICertificadoRepository> certificadoRepositoryMock;
            Mock<ICertificateManager> certificadoManagerMock;
            ConfigurarCertificadoDigital(out certificadoRepositoryMock, out certificadoManagerMock);

            Mock<IServiceFactory> serviceFactoryMock = ConfigurarServiceFactoryMock();

            ConfigurarConfiguracao();

            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            INotaFiscalRepository notaFiscalRepository = new NotaFiscalRepositoryFake();
            var certificadoRepository = certificadoRepositoryMock.Object;
            var serviceFactory = serviceFactoryMock.Object;
            var nfeConsulta = new Mock<INFeConsulta>().Object;
            var certificateManager = certificadoManagerMock.Object;
            var consultaStatusServicoService = new Mock<IConsultaStatusServicoFacade>().Object;

            var emissorService = new Mock<IEmissorService>().Object;
            var certificadoService = new Mock<ICertificadoService>().Object;
            var notaInutilizadaFacade = new Mock<InutilizarNotaFiscalFacade>().Object;
            var cancelaNotaFiscalService = new Mock<ICancelaNotaFiscalFacade>().Object;

            var notaFiscalContingenciaService = new EmiteEmiteNotaFiscalContingenciaFacade(configuracaoService, certificadoRepository, certificateManager, notaFiscalRepository, emissorService, nfeConsulta, serviceFactory, certificadoService, notaInutilizadaFacade, cancelaNotaFiscalService, new Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao }, new Utils.RijndaelManagedEncryption());

            var notaFiscalService = new EnviarNotaFiscalService(configuracaoRepository, notaFiscalRepository,
                certificadoRepository, configuracaoService, serviceFactory, nfeConsulta, certificateManager,
                notaFiscalContingenciaService, new Utils.RijndaelManagedEncryption());

            var modoOnlineService = new NotasFiscais.Services.ModoOnlineService(
                configuracaoRepository, consultaStatusServicoService, notaFiscalRepository,
                notaFiscalContingenciaService);

            // preencher nota fiscal;
            NotaFiscal notaFiscal = PreencherNotaFiscal();

            // Act

            notaFiscalService.EnviarNotaFiscal(notaFiscal, "000001", "E3BB2129-7ED0-31A10-CCB8-1B8BAC8FA2D0");

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

            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock.Setup(m => m.GetConfiguracao()).Returns(configuracao);
        }

        private static Mock<IServiceFactory> ConfigurarServiceFactoryMock()
        {
            var nfeAutorizacao4Mock = new Mock<NFeAutorizacao4Soap>();
            nfeAutorizacao4Mock
                .Setup(m => m.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Throws(new WebException());

            var serviceFactoryMock = new Mock<IServiceFactory>();
            serviceFactoryMock
                .Setup(m => m.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(),
                    It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>())).Returns(() =>
                    {
                        return new Service { SoapClient = nfeAutorizacao4Mock.Object };
                    });
            return serviceFactoryMock;
        }

        private static void ConfigurarCertificadoDigital(out Mock<ICertificadoRepository> certificadoRepositoryMock, out Mock<ICertificateManager> certificadoManagerMock)
        {
            certificadoRepositoryMock = new Mock<ICertificadoRepository>();
            certificadoRepositoryMock
                .Setup(m => m.GetCertificado())
                .Returns(() => new CertificadoEntity
                {
                    Caminho = "MyDevCert.pfx",
                    Nome = "MOCK NAME",
                    NumeroSerial = "1234",
                    Senha = "VqkVinLLG4/EAKUokpnVDg=="
                });

            var cert = new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            certificadoManagerMock = new Mock<ICertificateManager>();
            certificadoManagerMock
                .Setup(m => m.GetCertificateByPath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => cert);
        }

        private static NotaFiscal PreencherNotaFiscal()
        {
            var endereçoEmitente =
                new Endereco("QUADRA 200 CONJUNTO 20", "20", "BRASILIA", "BRASILIA", "70000000", "DF");
            var emitente = new Emissor("RAZAO SOCIAL", "NOME FANTASIA", "12345678998765", "1234567898765",
                "1234567898765", "4784900", "Regime Normal", endereçoEmitente, "99999999");
            var identificação = new IdentificacaoNFe(CodigoUfIbge.DF, DateTime.Now, emitente.CNPJ, Modelo.Modelo65, 1,
                "20887", TipoEmissao.Normal, Ambiente.Homologacao, emitente, "Venda", FinalidadeEmissao.Normal, true,
                PresencaComprador.Presencial, "CPF");
            var transporte = new Transporte(Modelo.Modelo65, null, null);
            const int valorTotalProdutos = 65;
            var totalIcms = new IcmsTotal(0, 0, 0, 0, 0, valorTotalProdutos, 0, 0, 0, 0, 0, 0, 0, 0, valorTotalProdutos,
                0);
            var totalNFe = new TotalNFe { IcmsTotal = totalIcms };
            var impostosList = new List<NotasFiscais.Entities.Imposto>
            {
                new NotasFiscais.Entities.Imposto {CST = "60", TipoImposto = TipoImposto.Icms},
                new NotasFiscais.Entities.Imposto {CST = "04", TipoImposto = TipoImposto.PIS}
            };

            var impostos = new NotasFiscais.Entities.Impostos(impostosList);

            var produtos = new List<Produto>
            {
                new Produto(impostos, 0, "5656", "0001", "GLP 13KG", "27111910", 1, "UN", 65, 0, false,0,0,0)
            };
            var pagamentos = new List<Pagamento>
            {
                new Pagamento(FormaPagamento.Dinheiro) {Valor = 65}
            };
            var infoAdicional = new InfoAdicional(produtos);
            var notaFiscal = new NotaFiscal(emitente, null, identificação, transporte, totalNFe, infoAdicional,
                produtos, pagamentos);
            return notaFiscal;
        }
    }
}