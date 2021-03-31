using System.Collections.Generic;
using System.Threading.Tasks;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Destinatario
{
    public class DestinatarioService : IDestinatarioService
    {
        private IDestinatarioRepository _destinatarioRepository;

        public DestinatarioService(IDestinatarioRepository destinatarioRepository)
        {
            _destinatarioRepository = destinatarioRepository;
        }

        public List<DestinatarioEntity> GetAll()
        {
            var destinatariosDB = _destinatarioRepository.GetAll();

            var destinatariosTO = new List<DestinatarioEntity>();
            foreach (var destinatarioDB in destinatariosDB) destinatariosTO.Add(destinatarioDB);

            return destinatariosTO;
        }

        public Task<List<DestinatarioEntity>> GetAllAsync()
        {
            return _destinatarioRepository.GetAllAsync();
        }

        public void ExcluirDestinatario(int id)
        {
            _destinatarioRepository.ExcluirDestinatario(id);
        }

        public DestinatarioEntity GetDestinatarioByID(int id)
        {
            return _destinatarioRepository.GetDestinatarioByID(id);
        }

        public int Salvar(DestinatarioEntity destinatarioEnt)
        {
            return _destinatarioRepository.Salvar(destinatarioEnt);
        }
    }
}