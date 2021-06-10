using System.Collections.Generic;
using NFe.Core.Cadastro.Ibpt;

namespace NFe.Core.Interfaces
{
    public interface IIbptRepository
    {
        IbptEntity GetValoresPorNCM(string ncmProduto);
        List<IbptEntity> GetValoresPorListaNCM(IEnumerable<string> ncmList);
    }
}