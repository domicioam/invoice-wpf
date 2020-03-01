using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IProdutoRepository
    {
        int Salvar(ProdutoEntity produto);
        void Excluir(ProdutoEntity produto);
        List<ProdutoEntity> GetAll();
        List<ProdutoEntity> GetProdutosByNaturezaOperacao(string descricao);
        ProdutoEntity GetProdutoByNcm(string ncm);
        ProdutoEntity GetByCodigo(string codigo);
    }
}