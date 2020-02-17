using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro.Produto
{
    public interface IProdutoService
    {
        List<ProdutoEntity> GetProdutosByNaturezaOperacao(string descricao);
        List<ProdutoEntity> GetAll();
        ProdutoEntity GetByCodigo(string codigo);
        void Salvar(ProdutoEntity produto);
    }
}