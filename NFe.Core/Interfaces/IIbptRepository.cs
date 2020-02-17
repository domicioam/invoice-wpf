using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IIbptRepository
    {
        IbptEntity GetValoresPorNCM(string ncmProduto);
        List<IbptEntity> GetValoresPorListaNCM(IEnumerable<string> ncmList);
    }
}