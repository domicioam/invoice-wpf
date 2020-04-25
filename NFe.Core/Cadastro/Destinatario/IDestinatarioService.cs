using System.Collections.Generic;
using System.Threading.Tasks;

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