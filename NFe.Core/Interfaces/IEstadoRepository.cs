using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IEstadoRepository
    {
        List<EstadoEntity> GetEstados();
    }
}