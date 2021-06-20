using System;
using System.Collections.Generic;
using System.Linq;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;

namespace NFe.Core.Interfaces
{
    public interface INotaInutilizadaRepository
    {
        int Salvar(NotaInutilizadaEntity notaInutilizada);
        NotaInutilizadaEntity GetNotaInutilizada(string idInutilizacao);
        void Insert(NotaInutilizadaEntity novaNotaInutilizada);
        void Save();
        List<NotaInutilizada> GetNotasFiscaisPorMesAno(DateTime periodo);
        NotaInutilizada GetNotaInutilizadaFromXml(string xml);
        NotaInutilizada GetNotaInutilizada(string idInutilizacao, bool isLoadXmlData);
        void Salvar(NotaInutilizada notaInutilizada, string xml);
    }
}