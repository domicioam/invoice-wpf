using GalaSoft.MvvmLight.Views;
using Moq;
using NFe.Core.Cadastro;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Transportadora;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Services;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NFe.WPF.UnitTests
{
    public class NFeViewModelTest : IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture _notaFiscalFixture;

        public NFeViewModelTest(NotaFiscalFixture notaFiscalFixture)
        {
            _notaFiscalFixture = notaFiscalFixture;
        }

        [Fact]
        public void Should_Send_Nota_Fiscal_When_Valid()
        {
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
            var notaFiscalService = new Mock<IEnviaNotaFiscalService>().Object;
            var configuracaoService = configuracaoServiceMock.Object;
            var emissorService = emissorServiceMock.Object;
            var produtoService = produtoServiceMock.Object;
            var estadoService = new Mock<IEstadoService>().Object;
            var destinatarioService = new Mock<IDestinatarioService>().Object;
            var municipioService = new Mock<MunicipioService>().Object;

            var destinatarioVM = new DestinatarioViewModel(estadoService, emissorService, destinatarioService, municipioService);

            var enviarNotaController = new NotaFiscal.ViewModel.EnviarNotaController(dialogService, notaFiscalService,
                configuracaoService, emissorService, produtoService);


            var vm = new NFeViewModel(enviarNotaController, dialogService, produtoService, new Mock<IEnviaNotaFiscalService>().Object, estadoService, emissorService, municipioService,
                new Mock<TransportadoraService>().Object, destinatarioService, new Mock<INaturezaOperacaoService>().Object, configuracaoService, destinatarioVM, new Mock<INotaFiscalRepository>().Object);

            vm.EnviarNota(_notaFiscalFixture.NFeRemessa, Modelo.Modelo55, new Mock<IClosable>().Object).Wait();
        } 
    }
}
