using System.Collections.Generic;
using System.Threading.Tasks;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro.Destinatario
{
    public interface IDestinatarioService
    {
        List<DestinatarioEntity> GetAll();
        Task<List<DestinatarioEntity>> GetAllAsync();
        void ExcluirDestinatario(int id);
        DestinatarioEntity GetDestinatarioByID(int id);
        int Salvar(DestinatarioEntity destinatarioEnt);
    }
}