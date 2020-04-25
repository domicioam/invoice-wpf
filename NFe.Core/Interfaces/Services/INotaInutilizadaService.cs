using System;
using System.Collections.Generic;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;

namespace NFe.Core.NotasFiscais.Services
{
    public interface INotaInutilizadaService
    {
        NotaInutilizadaTO GetNotaInutilizada(string idInutilizacao, bool isLoadXmlData, bool isProducao);
        NotaInutilizadaTO GetNotaInutilizadaFromXml(string xml);
        List<NotaInutilizadaTO> GetNotasFiscaisPorMesAno(DateTime periodo, bool isProducao);
        void Salvar(NotaInutilizadaTO notaInutilizada, string xml);
    }
}