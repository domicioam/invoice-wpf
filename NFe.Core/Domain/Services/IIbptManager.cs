using System.Collections.Generic;

namespace NFe.Core.Cadastro.Ibpt
{
    public interface IIbptManager
    {
        List<NotasFiscais.Ibpt> GetIbptByNcmList(List<string> ncmList);
    }
}