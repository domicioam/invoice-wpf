using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Imposto
{
    public class ImpostoService
    {
        private IGrupoImpostosRepository _grupoImpostosRepository;

        public ImpostoService(IGrupoImpostosRepository grupoImpostosRepository)
        {
            _grupoImpostosRepository = grupoImpostosRepository;
        }

        public List<GrupoImpostos> GetAll()
        {
            var impostosDB = _grupoImpostosRepository.GetAll();

            var impostos = new List<GrupoImpostos>();

            foreach (var impostoDb in impostosDB) impostos.Add(impostoDb);

            return impostos;
        }

        public void Salvar(GrupoImpostos imposto)
        {
            _grupoImpostosRepository.Salvar(imposto);
        }

        public GrupoImpostos GetById(int id)
        {
            return _grupoImpostosRepository.GetById(id);
        }
    }
}