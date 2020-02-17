using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Produto;
using NFe.Core.Entitities;

namespace NFe.WPF.ViewModel
{
    public class ProdutoMainViewModel
    {
        private readonly IProdutoService _produtoService;
        private readonly ProdutoViewModel _produtoViewModel;
        public ObservableCollection<ProdutoListItem> Produtos { get; set; }

        public ICommand AlterarProdutoCmd { get; set; }

        public ICommand LoadedCmd { get; set; }

        public ProdutoMainViewModel(IProdutoService produtoService, ProdutoViewModel produtoViewModel)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            Produtos = new ObservableCollection<ProdutoListItem>();
            AlterarProdutoCmd = new RelayCommand<ProdutoListItem>(AlterarProdutoCmd_Execute, null);

            _produtoService = produtoService;
            produtoViewModel.ProdutoAdicionadoEvent += ProdutoVM_ProdutoAdicionadoEvent;
            _produtoViewModel = produtoViewModel;
        }

        private void AlterarProdutoCmd_Execute(ProdutoListItem obj)
        {
            _produtoViewModel.AlterarProdutoCmd.Execute(obj.Codigo);
        }

        private void ProdutoVM_ProdutoAdicionadoEvent()
        {
            PopularListaProdutos();
        }

        private void LoadedCmd_Execute()
        {
            PopularListaProdutos();
        }

        private void PopularListaProdutos()
        {
            Produtos.Clear();

            var produtos = _produtoService.GetAll();

            foreach (var produtoDb in produtos)
            {
                var listItem = new ProdutoListItem()
                {
                    Ncm = produtoDb.NCM,
                    Id = produtoDb.Id,
                    Codigo = produtoDb.Codigo,
                    Descricao = produtoDb.Descricao,
                    Grupo = produtoDb.GrupoImpostos.Descricao,
                    UN = produtoDb.UnidadeComercial,
                    Valor = produtoDb.ValorUnitario.ToString("C2", new CultureInfo("pt-BR"))
                };

                Produtos.Add(listItem);
            }
        }
    }
}
