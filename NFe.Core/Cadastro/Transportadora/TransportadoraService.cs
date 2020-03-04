using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Transportadora
{
    public class TransportadoraService : ITransportadoraService
    {
        private ITransportadoraRepository _transportadoraRepository;

        public TransportadoraService(ITransportadoraRepository transportadoraRepository)
        {
            _transportadoraRepository = transportadoraRepository;
        }

        public List<TransportadoraEntity> GetAll()
        {
            var transportadorasDB = _transportadoraRepository.GetAll();

            var transportadoras = new List<TransportadoraEntity>();

            foreach (var transportadoraDB in transportadorasDB) transportadoras.Add(transportadoraDB);

            return transportadoras;
        }
    }
}