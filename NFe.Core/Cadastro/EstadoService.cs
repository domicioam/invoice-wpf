using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro
{
    public class EstadoService : IEstadoService
    {
        private IEstadoRepository _estadoRepository;

        public EstadoService(IEstadoRepository estadoRepository)
        {
            _estadoRepository = estadoRepository;
        }

        public List<EstadoEntity> GetEstados()
        {
            var estadosDB = _estadoRepository.GetEstados();

            var estados = new List<EstadoEntity>();

            foreach (var estadoDB in estadosDB)
            {
                var estado = new EstadoEntity();
                estado.CodigoUf = estadoDB.CodigoUf;
                estado.Id = estadoDB.Id;
                estado.Nome = estadoDB.Nome;
                estado.Regiao = estadoDB.Regiao;
                estado.Uf = estadoDB.Uf;

                estados.Add(estado);
            }

            return estados;
        }
    }
}