﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Assinatura;
using NFe.WPF.NotaFiscal.ViewModel;
using Xunit;
using Emissor = NFe.Core.NotasFiscais.Emissor;

namespace NFe.WPF.UnitTests
{
    public class EnviarNotaControllerTests : IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture _notaFiscalFixture;

        public EnviarNotaControllerTests(NotaFiscalFixture notaFiscalFixture)
        {
            _notaFiscalFixture = notaFiscalFixture;
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);
        }

        [Fact]
        public void NFCeModel_EnviarNota_Sucesso()
        {
            // Arrange



            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var emissorServiceMock = new Mock<IEmissorService>();
            emissorServiceMock
                .Setup(m => m.GetEmissor())
                .Returns(new Emissor(string.Empty, string.Empty, "98586321444578", string.Empty, string.Empty, string.Empty,
                    "Regime Normal",
                    new Endereco(string.Empty, string.Empty, string.Empty, "BRASILIA", string.Empty, "DF"),
                    string.Empty));

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetAll())
                .Returns(new List<ProdutoEntity>()
                {
                    new ProdutoEntity()
                    {
                        Id = 1,
                        ValorUnitario = 65,
                        Codigo = "0001",
                        Descricao = "Botijão P13",
                        GrupoImpostos = new GrupoImpostos()
                        {
                            Id = 1,
                            CFOP = "5656",
                            Descricao = "Gás Venda",
                            Impostos = _notaFiscalFixture.Impostos
                        },
                        GrupoImpostosId = 1,
                        NCM = "27111910",
                        UnidadeComercial = "UN"
                    }
                });

            var dialogService = new Mock<IDialogService>().Object;
            var notaFiscalService = new Mock<IEnviaNotaFiscalFacade>().Object;
            var configuracaoService = configuracaoServiceMock.Object;
            var emissorService = emissorServiceMock.Object;
            var produtoService = produtoServiceMock.Object;
            var configuracaoRepository = new Mock<IConfiguracaoRepository>().Object;
            var notaFiscalContigenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>().Object;
            var notaFiscalRepository = new Mock<INotaFiscalRepository>().Object;

            CertificadoEntity certificadoEntity = new CertificadoEntity
            {
                Caminho = "MyDevCert.pfx",
                Nome = "MOCK NAME",
                NumeroSerial = "1234",
                Senha = "VqkVinLLG4/EAKUokpnVDg=="
            };

            var certificadoRepositoryMock = new Mock<ICertificadoRepository>();
            certificadoRepositoryMock
                .Setup(m => m.GetCertificado())
                .Returns(() => certificadoEntity);

            var cert = new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            certificadoRepositoryMock.Setup(m => m.PickCertificateBasedOnInstallationType(certificadoEntity))
                .Returns(() => cert);

            var certificadoManagerMock = new Mock<ICertificateManager>();
            certificadoManagerMock
                .Setup(m => m.GetCertificateByPath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => cert);

            Core.Sefaz.SefazSettings sefazSettings = new Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao };
            var enviarNotaController = new NotaFiscal.ViewModel.EnviarNotaAppService(dialogService, notaFiscalService, configuracaoService, emissorService, 
                produtoService, sefazSettings, configuracaoRepository, notaFiscalContigenciaService, notaFiscalRepository, certificadoRepositoryMock.Object);

            // Act

            enviarNotaController.EnviarNota(_notaFiscalFixture.NFCeModel, Modelo.Modelo65).Wait();
        }

        [Fact]
        public void NFeModel_EnviarNota_Sucesso()
        {
            // Arrange

            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var emissorServiceMock = new Mock<IEmissorService>();
            emissorServiceMock
                .Setup(m => m.GetEmissor())
                .Returns(new Emissor(string.Empty, string.Empty, "98586321444578", string.Empty, string.Empty, string.Empty,
                    "Regime Normal",
                    new Endereco(string.Empty, string.Empty, string.Empty, "BRASILIA", string.Empty, "DF"),
                    string.Empty));

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetAll())
                .Returns(new List<ProdutoEntity>()
                {
                    new ProdutoEntity()
                    {
                        Id = 1,
                        ValorUnitario = 65,
                        Codigo = "0001",
                        Descricao = "Botijão P13",
                        GrupoImpostos = new GrupoImpostos()
                        {
                            Id = 1,
                            CFOP = "5656",
                            Descricao = "Gás Venda",
                            Impostos = _notaFiscalFixture.Impostos
                        },
                        GrupoImpostosId = 1,
                        NCM = "27111910",
                        UnidadeComercial = "UN"
                    }
                });

            var dialogService = new Mock<IDialogService>().Object;
            var notaFiscalService = new Mock<IEnviaNotaFiscalFacade>().Object;
            var configuracaoService = configuracaoServiceMock.Object;
            var emissorService = emissorServiceMock.Object;
            var produtoService = produtoServiceMock.Object;
            var configuracaoRepository = new Mock<IConfiguracaoRepository>().Object;
            var notaFiscalContigenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>().Object;
            var notaFiscalRepository = new Mock<INotaFiscalRepository>().Object;
            var certificadoRepository = new Mock<ICertificadoRepository>().Object;

            Core.Sefaz.SefazSettings sefazSettings = new Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao };
            var enviarNotaController = new NotaFiscal.ViewModel.EnviarNotaAppService(dialogService, notaFiscalService, configuracaoService, emissorService,
                produtoService, sefazSettings, configuracaoRepository, notaFiscalContigenciaService, notaFiscalRepository, certificadoRepository);

            // Act

            enviarNotaController.EnviarNota(_notaFiscalFixture.NFeModelWithPagamento, Modelo.Modelo55).Wait();
        }

        [Fact]
        public async Task NFeModel_EnviarNota_ArgumentExceptionValorTotalInválido()
        {
            // Arrange

            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var emissorServiceMock = new Mock<IEmissorService>();
            emissorServiceMock
                .Setup(m => m.GetEmissor())
                .Returns(new Emissor(string.Empty, string.Empty, "98586321444578", string.Empty, string.Empty, string.Empty,
                    "Regime Normal",
                    new Endereco(string.Empty, string.Empty, string.Empty, "BRASILIA", string.Empty, "DF"),
                    string.Empty));

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetAll())
                .Returns(new List<ProdutoEntity>()
                {
                    new ProdutoEntity()
                    {
                        Id = 1,
                        ValorUnitario = 65,
                        Codigo = "0001",
                        Descricao = "Botijão P13",
                        GrupoImpostos = new GrupoImpostos()
                        {
                            Id = 1,
                            CFOP = "5656",
                            Descricao = "Gás Venda",
                            Impostos = _notaFiscalFixture.Impostos
                        },
                        GrupoImpostosId = 1,
                        NCM = "27111910",
                        UnidadeComercial = "UN"
                    }
                });

            var dialogService = new Mock<IDialogService>().Object;
            var notaFiscalService = new Mock<IEnviaNotaFiscalFacade>().Object;
            var configuracaoService = configuracaoServiceMock.Object;
            var emissorService = emissorServiceMock.Object;
            var produtoService = produtoServiceMock.Object;
            var configuracaoRepository = new Mock<IConfiguracaoRepository>().Object;
            var notaFiscalContigenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>().Object;
            var notaFiscalRepository = new Mock<INotaFiscalRepository>().Object;
            var certificadoRepository = new Mock<ICertificadoRepository>().Object;

            Core.Sefaz.SefazSettings sefazSettings = new Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao };
            var enviarNotaController = new NotaFiscal.ViewModel.EnviarNotaAppService(dialogService, notaFiscalService, configuracaoService, emissorService,
                produtoService, sefazSettings, configuracaoRepository, notaFiscalContigenciaService, notaFiscalRepository, certificadoRepository);

            // Act

            await Assert.ThrowsAnyAsync<ArgumentException>(() => enviarNotaController.EnviarNota(_notaFiscalFixture.NFeTotalInvalido, Modelo.Modelo55));
        }

        [Fact]
        public async Task NFeModel_EnviarNota_Should_Throw_Exception_When_NotaFiscal_Is_Has_Errors()
        {
            // Arrange

            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var emissorServiceMock = new Mock<IEmissorService>();
            emissorServiceMock
                .Setup(m => m.GetEmissor())
                .Returns(new Emissor(string.Empty, string.Empty, "98586321444578", string.Empty, string.Empty, string.Empty,
                    "Regime Normal",
                    new Endereco(string.Empty, string.Empty, string.Empty, "BRASILIA", string.Empty, "DF"),
                    string.Empty));

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetAll())
                .Returns(new List<ProdutoEntity>()
                {
                    new ProdutoEntity()
                    {
                        Id = 1,
                        ValorUnitario = 65,
                        Codigo = "0001",
                        Descricao = "Botijão P13",
                        GrupoImpostos = new GrupoImpostos()
                        {
                            Id = 1,
                            CFOP = "5656",
                            Descricao = "Gás Venda",
                            Impostos = _notaFiscalFixture.Impostos
                        },
                        GrupoImpostosId = 1,
                        NCM = "27111910",
                        UnidadeComercial = "UN"
                    }
                });

            var dialogService = new Mock<IDialogService>().Object;
            var notaFiscalService = new Mock<IEnviaNotaFiscalFacade>().Object;
            var configuracaoService = configuracaoServiceMock.Object;
            var emissorService = emissorServiceMock.Object;
            var produtoService = produtoServiceMock.Object;
            var configuracaoRepository = new Mock<IConfiguracaoRepository>().Object;
            var notaFiscalContigenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>().Object;
            var notaFiscalRepository = new Mock<INotaFiscalRepository>().Object;
            var certificadoRepository = new Mock<ICertificadoRepository>().Object;

            Core.Sefaz.SefazSettings sefazSettings = new Core.Sefaz.SefazSettings() { Ambiente = Ambiente.Homologacao };
            var enviarNotaController = new NotaFiscal.ViewModel.EnviarNotaAppService(dialogService, notaFiscalService, configuracaoService, emissorService,
                produtoService, sefazSettings, configuracaoRepository, notaFiscalContigenciaService, notaFiscalRepository, certificadoRepository);

            // Act

            await Assert.ThrowsAnyAsync<NotaFiscalModelHasErrorsException>(() => enviarNotaController.EnviarNota(_notaFiscalFixture.NFeModelWithErrors, Modelo.Modelo55));
        }
    }
}
