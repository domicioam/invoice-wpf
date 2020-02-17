using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Produto
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public List<ProdutoEntity> GetProdutosByNaturezaOperacao(string descricao)
        {
            var produtosDB = _produtoRepository.GetProdutosByNaturezaOperacao(descricao);

            var produtos = new List<ProdutoEntity>();

            foreach (var produtoDB in produtosDB) produtos.Add(produtoDB);

            return produtos;
        }

        public List<ProdutoEntity> GetAll()
        {
            var produtosDB = _produtoRepository.GetAll();

            var produtosTO = new List<ProdutoEntity>();

            foreach (var produtoDB in produtosDB) produtosTO.Add(produtoDB);

            return produtosTO;
        }

        public ProdutoEntity GetByCodigo(string codigo)
        {
            return _produtoRepository.GetByCodigo(codigo);
        }

        public void Salvar(ProdutoEntity produto)
        {
            _produtoRepository.Salvar(produto);
        }
    }
}