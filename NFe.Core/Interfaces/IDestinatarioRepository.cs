using System.Collections.Generic;
using System.Threading.Tasks;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IDestinatarioRepository
    {
        int Salvar(DestinatarioEntity destinatario);
        List<DestinatarioEntity> GetAll();
        Task<List<DestinatarioEntity>> GetAllAsync();
        DestinatarioEntity GetDestinatarioByID(int id);
        void ExcluirDestinatario(int id);
    }
}