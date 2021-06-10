using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface INaturezaOperacaoRepository
    {
        NaturezaOperacaoEntity GetNaturezaOperacaoPorCfop(string cfop);
        List<NaturezaOperacaoEntity> GetAll();
    }
}