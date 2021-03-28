using System;
using System.Collections.Generic;

namespace NFe.Core.NotasFiscais.Services
{
    public interface INotaInutilizadaService
    {
        NotaInutilizadaTO GetNotaInutilizada(string idInutilizacao, bool isLoadXmlData);
        NotaInutilizadaTO GetNotaInutilizadaFromXml(string xml);
        List<NotaInutilizadaTO> GetNotasFiscaisPorMesAno(DateTime periodo);
        void Salvar(NotaInutilizadaTO notaInutilizada, string xml);
    }
}