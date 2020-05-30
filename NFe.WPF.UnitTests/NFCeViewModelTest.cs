using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmissorNFe.VO;
using GalaSoft.MvvmLight.Views;
using Moq;
using NFe.Core.Cadastro;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.WPF.Model;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Services;
using Xunit;
using Imposto = NFe.Core.Entitities.Imposto;

namespace NFe.WPF.UnitTests
{
    public class NFCeViewModelTest : IClassFixture<NotaFiscalFixture>
    {
        //TODO: REGISTRAR NOVAS INTERFACES

        private readonly NotaFiscalFixture _notaFiscalFixture;

        public NFCeViewModelTest(NotaFiscalFixture notaFiscalFixture)
        {
            _notaFiscalFixture = notaFiscalFixture;
        }

        [Fact]
        public void Deveria_gerar_pagamento()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.LoadedCmd.Execute("55");
            var pagamento = new PagamentoVO() { FormaPagamento = "Dinheiro", QtdeParcelas = 1, ValorParcela = 10, ValorTotal = "10" };
            nfce.Pagamento = pagamento;
            nfce.GerarPagtoCmd.Execute(null);
            Assert.Contains(pagamento, nfce.NotaFiscal.Pagamentos);
        }

        [Fact]
        public void Deveria_excluir_pagamento_gerado()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.LoadedCmd.Execute("55");
            var pagamento = new PagamentoVO() { FormaPagamento = "Dinheiro", QtdeParcelas = 1, ValorParcela = 10, ValorTotal = "10" };
            nfce.Pagamento = pagamento;
            nfce.GerarPagtoCmd.Execute(null);
            nfce.ExcluirPagamentoCmd.Execute(pagamento);
            Assert.DoesNotContain(pagamento, nfce.NotaFiscal.Pagamentos);
            Assert.Empty(nfce.NotaFiscal.Pagamentos);
            Assert.Equal(pagamento.ValorParcela * pagamento.QtdeParcelas, nfce.Pagamento.ValorParcela);
        }

        [Fact]
        public void Deveria_adicionar_produto()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            var totalLiquido = 65;
            var produto = new ProdutoVO() {Descontos = 0, Descricao = "Botijão P13", Frete = 0, Outros = 0, ProdutoSelecionado = _notaFiscalFixture.ProdutoEntity, QtdeProduto = 1, Seguro = 0, TotalBruto = 0, ValorUnitario = 0};
            produto.TotalLiquido = totalLiquido;

            nfce.LoadedCmd.Execute("55");
            nfce.Produto = produto;

            nfce.AdicionarProdutoCmd.Execute(null);

            Assert.Contains(produto, nfce.NotaFiscal.Produtos);
            Assert.Equal(totalLiquido, nfce.Pagamento.ValorParcela);
        }

        [Fact]
        public void Deveria_enviar_nota_fiscal()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var enviarNotaMock = new Mock<IEnviarNota>();
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                enviarNotaMock.Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.NotaFiscal = _notaFiscalFixture.NFCeModel;

            nfce.EnviarNotaCmd.Execute(new Mock<IClosable>().Object);
            enviarNotaMock.Verify(m => m.EnviarNota(It.IsNotNull<NotaFiscalModel>(), It.IsAny<Core.NotasFiscais.Modelo>()), Times.Once);
        }

        [Fact]
        public void Deveria_imprimir_nota_fiscal_quando_resposta_de_imprimir_é_sim()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var enviarNotaMock = new Mock<IEnviarNota>();
            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns(Task.FromResult(true));
            var nfce = new NFCeViewModel(dialogServiceMock.Object,
                enviarNotaMock.Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.NotaFiscal = _notaFiscalFixture.NFCeModel;

            nfce.EnviarNotaCmd.Execute(new Mock<IClosable>().Object);
            enviarNotaMock.Verify(m => m.EnviarNota(It.IsAny<NotaFiscalModel>(), It.IsAny<Core.NotasFiscais.Modelo>()), Times.Once);
            enviarNotaMock.Verify(m => m.ImprimirNotaFiscal(It.IsAny<Core.NotasFiscais.NotaFiscal>()), Times.Once);
        }

        [Fact]
        public void Deveria_exibir_erro_quando_argument_exception_por_nota_fiscal_inválida()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var enviarNotaMock = new Mock<IEnviarNota>();
            enviarNotaMock.Setup(m => m.EnviarNota(It.IsAny<NotaFiscalModel>(), It.IsAny<Modelo>()))
                .Throws(new ArgumentException());
            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns(Task.FromResult(true));
            var nfce = new NFCeViewModel(dialogServiceMock.Object,
                enviarNotaMock.Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.NotaFiscal = _notaFiscalFixture.NFCeModel;
            nfce.EnviarNotaCmd.Execute(new Mock<IClosable>().Object);

            dialogServiceMock.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null), Times.AtLeastOnce);
        }

        [Fact]
        public void Deveria_exibir_erro_quando_ocorrer_exception_genérica()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var enviarNotaMock = new Mock<IEnviarNota>();
            enviarNotaMock.Setup(m => m.EnviarNota(It.IsAny<NotaFiscalModel>(), It.IsAny<Modelo>()))
                .Throws(new Exception());
            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns(Task.FromResult(true));
            var nfce = new NFCeViewModel(dialogServiceMock.Object,
                enviarNotaMock.Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.NotaFiscal = _notaFiscalFixture.NFCeModel;
            nfce.EnviarNotaCmd.Execute(new Mock<IClosable>().Object);

            dialogServiceMock.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null), Times.AtLeastOnce);
        }

        [Fact]
        public void Deveria_atualizar_valor_parcela_quando_produto_é_removido()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            var totalLiquido = 65;
            var produto = new ProdutoVO() { Descontos = 0, Descricao = "Botijão P13", Frete = 0, Outros = 0, ProdutoSelecionado = _notaFiscalFixture.ProdutoEntity, QtdeProduto = 1, Seguro = 0, TotalBruto = 0, ValorUnitario = 0 };
            produto.TotalLiquido = totalLiquido;

            nfce.LoadedCmd.Execute("55");
            nfce.Produto = produto;

            nfce.AdicionarProdutoCmd.Execute(null);

            Assert.Contains(produto, nfce.NotaFiscal.Produtos);
            Assert.Equal(totalLiquido, nfce.Pagamento.ValorParcela);

            nfce.ExcluirProdutoNotaCmd.Execute(produto);
            Assert.Equal(0, nfce.Pagamento.ValorParcela);
        }

        [Fact]
        public void Deveria_limpar_lista_pagamentos_quando_produto_é_removido()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            var totalLiquido = 65;
            var produto = new ProdutoVO() { Descontos = 0, Descricao = "Botijão P13", Frete = 0, Outros = 0, ProdutoSelecionado = _notaFiscalFixture.ProdutoEntity, QtdeProduto = 1, Seguro = 0, TotalBruto = 0, ValorUnitario = 0 };
            produto.TotalLiquido = totalLiquido;

            nfce.LoadedCmd.Execute("55");
            nfce.Produto = produto;

            nfce.AdicionarProdutoCmd.Execute(null);

            var pagamento = new PagamentoVO() { FormaPagamento = "Dinheiro", QtdeParcelas = 1, ValorParcela = totalLiquido, ValorTotal = totalLiquido.ToString() };
            nfce.Pagamento = pagamento;
            nfce.GerarPagtoCmd.Execute(null);

            nfce.ExcluirProdutoNotaCmd.Execute(produto);
            Assert.Equal(0, nfce.Pagamento.ValorParcela);
            Assert.Empty(nfce.NotaFiscal.Pagamentos);
        }

        [Fact]
        public void LoadedCmd_deveria_carregar_destinatários_para_modelo_65()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>() { new DestinatarioEntity() });
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            nfce.LoadedCmd.Execute("65");
            Assert.NotEmpty(nfce.Destinatarios);
        }

        [Fact]
        public void ClosedCmd_deveria_limpar_view_model()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>() { new DestinatarioEntity() });
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            var totalLiquido = 65;
            var produto = new ProdutoVO() { Descontos = 0, Descricao = "Botijão P13", Frete = 0, Outros = 0, ProdutoSelecionado = _notaFiscalFixture.ProdutoEntity, QtdeProduto = 1, Seguro = 0, TotalBruto = 0, ValorUnitario = 0 };
            produto.TotalLiquido = totalLiquido;

            nfce.LoadedCmd.Execute("65");
            nfce.Produto = produto;
            nfce.AdicionarProdutoCmd.Execute(null);
            nfce.ClosedCmd.Execute(null);

            Assert.Empty(nfce.Destinatarios);
            Assert.Empty(nfce.ProdutosCombo);
            Assert.Null(nfce.NotaFiscal);
            Assert.Null(nfce.Produto.Descricao);
        }

        [Fact]
        public void Deveria_atualizar_valor_da_parcela_quando_produto_é_removido_e_contém_dois_produtos()
        {
            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(m => m.GetConfiguracao())
                .Returns(new ConfiguracaoEntity());

            var produtoServiceMock = new Mock<IProdutoRepository>();
            produtoServiceMock
                .Setup(m => m.GetProdutosByNaturezaOperacao(It.IsAny<string>()))
                .Returns(new List<ProdutoEntity>()
                {
                    _notaFiscalFixture.ProdutoEntity
                });

            var destinatarioServiceMock = new Mock<IDestinatarioService>();
            destinatarioServiceMock.Setup(m => m.GetAll())
                .Returns(new List<DestinatarioEntity>());
            var destinatario = new DestinatarioViewModel(new Mock<IEstadoRepository>().Object, new Mock<IEmissorService>().Object, destinatarioServiceMock.Object, new Mock<IMunicipioRepository>().Object);
            var nfce = new NFCeViewModel(new Mock<IDialogService>().Object,
                new Mock<IEnviarNota>().Object, new Mock<INaturezaOperacaoService>().Object,
                configuracaoServiceMock.Object, produtoServiceMock.Object,
                destinatarioServiceMock.Object);

            var totalLiquido = 65;
            var produto = new ProdutoVO() { Descontos = 0, Descricao = "Botijão P13", Frete = 0, Outros = 0, ProdutoSelecionado = _notaFiscalFixture.ProdutoEntity, QtdeProduto = 1, Seguro = 0, TotalBruto = 0, ValorUnitario = 0 };
            produto.TotalLiquido = totalLiquido;

            nfce.LoadedCmd.Execute("55");
            nfce.Produto = produto;

            nfce.AdicionarProdutoCmd.Execute(null);
            nfce.Produto = produto;
            nfce.AdicionarProdutoCmd.Execute(null);

            nfce.ExcluirProdutoNotaCmd.Execute(produto);
            Assert.Equal(65, nfce.Pagamento.ValorParcela);
        }
    }
}
