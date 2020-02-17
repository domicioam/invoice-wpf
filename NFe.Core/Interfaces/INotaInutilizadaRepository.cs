using System;
using System.Linq;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface INotaInutilizadaRepository
    {
        int Salvar(NotaInutilizadaEntity notaInutilizada);
        IQueryable<NotaInutilizadaEntity> GetNotasFiscaisPorMesAno(DateTime mesAno, bool isProducao);
        NotaInutilizadaEntity GetNotaInutilizada(string idInutilizacao, bool isProducao);
        void Insert(NotaInutilizadaEntity novaNotaInutilizada);
        void Save();
    }
}