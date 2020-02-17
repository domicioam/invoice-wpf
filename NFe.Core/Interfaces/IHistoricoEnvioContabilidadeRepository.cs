using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IHistoricoEnvioContabilidadeRepository
    {
        int Salvar(HistoricoEnvioContabilidade emitente);
        Task<List<HistoricoEnvioContabilidade>> GetAllAsync();
        Task<int> GetHistoricoByPeriodoAsync(DateTime periodo);
    }
}