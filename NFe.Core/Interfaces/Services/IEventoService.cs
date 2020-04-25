using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IEventoService
    {
        EventoEntity GetEventoCancelamentoFromXml(string xml, string file, INotaFiscalRepository notaFiscalRepository);
        EventoEntity GetEventoPorNota(int idNota, bool isLoadXmlData);
        List<EventoEntity> GetEventosPorNotasId(IEnumerable<int> idsNotas, bool isLoadXmlData);
        void Salvar(EventoEntity EventoEntity);
    }
}