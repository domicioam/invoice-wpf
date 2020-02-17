using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface ITransportadoraRepository
    {
        int Salvar(TransportadoraEntity transportadora);
        void Excluir(int id);
        List<TransportadoraEntity> GetAll();
    }
}