using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro
{
    public interface IMunicipioService
    {
        List<MunicipioEntity> GetMunicipioByUf(string uf);
    }
}