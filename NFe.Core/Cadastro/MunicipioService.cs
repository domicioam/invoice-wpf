using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro
{
    public class MunicipioService : IMunicipioService
    {
        private IMunicipioRepository _municipioRepository;

        public MunicipioService(IMunicipioRepository municipioRepository)
        {
            _municipioRepository = municipioRepository;
        }

        public List<MunicipioEntity> GetMunicipioByUf(string uf)
        {
            var municipiosDB = _municipioRepository.GetMunicipioByUf(uf);

            var municipios = new List<MunicipioEntity>();

            foreach (var municipioDB in municipiosDB)
            {
                var municipio = new MunicipioEntity
                {
                    Codigo = municipioDB.Codigo,
                    Id = municipioDB.Id,
                    Nome = municipioDB.Nome,
                    Uf = municipioDB.Uf
                };

                municipios.Add(municipio);
            }

            return municipios;
        }
    }
}