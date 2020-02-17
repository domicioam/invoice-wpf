using EmissorNFe.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFe.Core.Domain.Services.Produto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NFe.WPF.ViewModel.Services;

namespace NFe.UnitTests.ViewModel
{
    [TestClass]
    public class ProdutoViewModelTest
    {
        [TestMethod]
        public void AlterarProduto_ShouldReturnSuccess()
        {
            //Arrange

            var viewModel = new ProdutoViewModel();
            var produto = ProdutoService.GetByCodigo("0001");
            Mock<IClosable> closableMock = new Mock<IClosable>();

            //Act

            viewModel.Id = produto.Id;
            viewModel.CodigoItem = produto.Codigo;
            viewModel.Descricao = produto.Descricao;
            viewModel.NCM = produto.NCM;
            viewModel.UnidadeComercial = produto.UnidadeComercial;
            viewModel.ValorUnitario = produto.ValorUnitario;
            viewModel.Imposto = produto.GrupoImpostos;

            viewModel.ValorUnitario = 58.00;
            viewModel.SalvarCmd.Execute(closableMock.Object);

            //Assert

            produto = ProdutoService.GetByCodigo("0001");
            Assert.AreEqual(produto.ValorUnitario, 58.00);
        }
    }
}
