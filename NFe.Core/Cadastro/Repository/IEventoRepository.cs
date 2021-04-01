using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IEventoRepository
    {
        void Excluir(EventoEntity evento);
        List<EventoEntity> GetAll();
        List<EventoEntity> GetEventosPorNotasId(IEnumerable<int> idsNotas);
        EventoEntity GetEventoPorNota(int idNota);
        void Insert(EventoEntity obj);
        void Save();
        void Salvar(EventoEntity eventoEntity);
        List<EventoEntity> GetEventosPorNotasId(IEnumerable<int> idsNotas, bool isLoadXmlData);
        EventoEntity GetEventoCancelamentoFromXml(string xml, string file, INotaFiscalRepository notaFiscalRepository);
        EventoEntity GetEventoPorNota(int idNota, bool isLoadXmlData);
    }
}