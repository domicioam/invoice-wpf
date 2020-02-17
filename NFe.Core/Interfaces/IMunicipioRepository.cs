using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IMunicipioRepository
    {
        List<MunicipioEntity> GetMunicipioByUf(string uf);
    }
}