using NFe.Core.Entitities;
using NFe.Core.Domain;

namespace NFe.Core.Cadastro.Emissor
{
    public interface IEmissorService
    {
        Domain.Emissor GetEmissor();
        EmitenteEntity GetEmitenteEntity();
        void Salvar(EmitenteEntity emitente);
    }
}