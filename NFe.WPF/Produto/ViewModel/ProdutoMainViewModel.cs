﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.WPF.ViewModel
{
    public class ProdutoMainViewModel
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ProdutoViewModel _produtoViewModel;
        public ObservableCollection<ProdutoListItem> Produtos { get; set; }

        public ICommand AlterarProdutoCmd { get; set; }

        public ICommand LoadedCmd { get; set; }

        public ProdutoMainViewModel(IProdutoRepository produtoRepository, ProdutoViewModel produtoViewModel)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            Produtos = new ObservableCollection<ProdutoListItem>();
            AlterarProdutoCmd = new RelayCommand<ProdutoListItem>(AlterarProdutoCmd_Execute, null);

            _produtoRepository = produtoRepository;
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

            var produtos = _produtoRepository.GetAll();

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