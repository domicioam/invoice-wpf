using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro
{
    public interface IEstadoService
    {
        List<EstadoEntity> GetEstados();
    }
}