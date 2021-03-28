using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IEventoRepository
    {
        int Salvar(EventoEntity evento);
        void Excluir(EventoEntity evento);
        List<EventoEntity> GetAll();
        List<EventoEntity> GetEventosPorNotasId(IEnumerable<int> idsNotas);
        EventoEntity GetEventoPorNota(int idNota);
        void Insert(EventoEntity obj);
        void Save();
    }
}