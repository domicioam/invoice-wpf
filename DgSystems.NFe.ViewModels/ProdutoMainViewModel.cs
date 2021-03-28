using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.WPF.Events;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace NFe.WPF.ViewModel
{
    public class ProdutoMainViewModel
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDialogService _dialogService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ProdutoViewModel _produtoViewModel;
        public ObservableCollection<ProdutoListItem> Produtos { get; set; }

        public ICommand AlterarProdutoCmd { get; set; }
        public ICommand RemoverProdutoCmd { get; set; }

        public ICommand LoadedCmd { get; set; }

        public ProdutoMainViewModel(IProdutoRepository produtoRepository, ProdutoViewModel produtoViewModel, IDialogService dialogService)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            Produtos = new ObservableCollection<ProdutoListItem>();
            AlterarProdutoCmd = new RelayCommand<ProdutoListItem>(AlterarProdutoCmd_Execute, null);
            RemoverProdutoCmd = new RelayCommand<ProdutoListItem>(RemoverProdutoCmd_Execute, null);
            _dialogService = dialogService;

            _produtoRepository = produtoRepository;

            MessagingCenter.Subscribe<ProdutoViewModel, ProdutoAdicionadoEvent>(this, nameof(ProdutoAdicionadoEvent), (s, e) => ProdutoVM_ProdutoAdicionadoEvent());

            _produtoViewModel = produtoViewModel;
        }

        private async void RemoverProdutoCmd_Execute(ProdutoListItem obj)
        {
            try
            {
                var produto = _produtoRepository.GetByCodigo(obj.Codigo);
                _produtoRepository.Excluir(produto);
                PopularListaProdutos();
            }
            catch (Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Ocorreu um erro ao remover o produto desejado.", "Erro", "OK", null);
            }
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

            foreach (var produtoDb in _produtoRepository.GetAll())
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
