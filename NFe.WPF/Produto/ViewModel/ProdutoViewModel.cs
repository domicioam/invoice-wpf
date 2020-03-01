using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Produto;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.WPF.ViewModel.Services;

namespace NFe.WPF.ViewModel
{
    public delegate void ProdutoAdicionadoEventHandler();

    public class ProdutoViewModel : ViewModelBase
    {
        private readonly IProdutoRepository _produtoRepository;

        public event ProdutoAdicionadoEventHandler ProdutoAdicionadoEvent = delegate { };
        public int Id { get; set; }
        public string CodigoItem { get; set; }
        public string Descricao { get; set; }
        public string UnidadeComercial { get; set; }
        public double ValorUnitario { get; set; }
        public string NCM { get; set; }
        public List<string> UnidadesComerciais { get; set; }

        public ICommand AlterarProdutoCmd { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand SalvarCmd { get; set; }
        public ICommand CancelarCmd { set; get; }

        public ProdutoViewModel(IProdutoRepository produtoRepository)
        {
            UnidadesComerciais = new List<string>() { "UN" };

            AlterarProdutoCmd = new RelayCommand<string>(AlterarProduto_Execute, null);
            SalvarCmd = new RelayCommand<IClosable>(SalvarCmd_Execute, null);
            CancelarCmd = new RelayCommand<object>(CancelarCmd_Execute, null);
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _produtoRepository = produtoRepository;
        }

        private void AlterarProduto_Execute(string produtoCodigo)
        {
            var produto = _produtoRepository.GetByCodigo(produtoCodigo);

            Id = produto.Id;
            CodigoItem = produto.Codigo;
            Descricao = produto.Descricao;
            UnidadeComercial = produto.UnidadeComercial;
            ValorUnitario = produto.ValorUnitario;
            NCM = produto.NCM;

            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new CadastroProdutoWindow() { Owner = mainWindow }.ShowDialog();
        }

        private void LoadedCmd_Execute()
        {
        }

        private void CancelarCmd_Execute(object obj)
        {
            throw new NotImplementedException();
        }

        private void SalvarCmd_Execute(IClosable window)
        {
            var produto = _produtoRepository.GetByCodigo(CodigoItem) ?? new ProdutoEntity();

            produto.Id = Id;
            produto.Codigo = CodigoItem;
            produto.Descricao = Descricao;
            produto.NCM = NCM;
            produto.UnidadeComercial = UnidadeComercial;
            produto.ValorUnitario = ValorUnitario;

            _produtoRepository.Salvar(produto);
            ProdutoAdicionadoEvent();
            window.Close();
        }
    }
}
