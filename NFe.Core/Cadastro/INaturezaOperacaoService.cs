using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro
{
    public interface INaturezaOperacaoService
    {
        string GetNaturezaOperacaoPorCfop(string cfop);
        List<NaturezaOperacaoEntity> GetAll();
    }
}