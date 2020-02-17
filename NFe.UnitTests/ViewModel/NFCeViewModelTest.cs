using EmissorNFe.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NFe.WPF.ViewModel.Controllers;
using GalaSoft.MvvmLight.Views;
using NFe.WPF.ViewModel.Services;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Domain.Services.NotaFiscal;
using NFe.Core.TO;

namespace NFe.UnitTests.ViewModel
{
    [TestClass]
    public class NFCeViewModelTest
    {
        ISplashScreenService _splashScreenService;
        IDialogService _dialogService;
        IEnviarNota _enviarNotaController;
        INotaFiscalService _notaFiscalService;

        [TestMethod]
        public async Task NFCeShouldBeSentTest()
        {
            //Arrange
            Mock<ISplashScreenService> splashMock = new Mock<ISplashScreenService>();
            Mock<IDialogService> dialogMock = new Mock<IDialogService>();
            Mock<IClosable> closableMock = new Mock<IClosable>();
            Mock<INotaFiscalService> notaFiscalServiceMock = new Mock<INotaFiscalService>();

            notaFiscalServiceMock.Setup(m => m.EnviarNotaFiscalAsync(It.IsAny<NotaFiscal>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(0);

            _splashScreenService = splashMock.Object;
            _dialogService = dialogMock.Object;
            _notaFiscalService = notaFiscalServiceMock.Object;
            _enviarNotaController = new EnviarNotaController(_splashScreenService, _dialogService, _notaFiscalService);

            var viewModel = new NFCeViewModel(new DestinatarioViewModel(), _splashScreenService, _dialogService, _enviarNotaController);
            viewModel.LoadedCmd.Execute(null);
            viewModel.Produto.QtdeProduto = 1;
            viewModel.Produto.ProdutoSelecionado = viewModel.ProdutosCombo[0];
            viewModel.AdicionarProdutoCmd.Execute(null);
            viewModel.GerarPagtoCmd.Execute(null);

            //Act
            await viewModel.EnviarNotaCmd.ExecuteAsync(closableMock.Object);

            //Assert
            notaFiscalServiceMock.Verify(n => n.EnviarNotaFiscalAsync(It.IsAny<NotaFiscal>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            closableMock.Verify(c => c.Close(), Times.Once);
        }
    }
}
