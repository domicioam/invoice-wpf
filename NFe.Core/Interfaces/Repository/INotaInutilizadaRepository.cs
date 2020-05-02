using System;
using System.Linq;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface INotaInutilizadaRepository
    {
        int Salvar(NotaInutilizadaEntity notaInutilizada);
        IQueryable<NotaInutilizadaEntity> GetNotasFiscaisPorMesAno(DateTime mesAno);
        NotaInutilizadaEntity GetNotaInutilizada(string idInutilizacao);
        void Insert(NotaInutilizadaEntity novaNotaInutilizada);
        void Save();
    }
}