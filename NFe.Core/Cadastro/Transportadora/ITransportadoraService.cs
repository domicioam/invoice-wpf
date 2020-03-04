using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro.Transportadora
{
    public interface ITransportadoraService
    {
        List<TransportadoraEntity> GetAll();
    }
}