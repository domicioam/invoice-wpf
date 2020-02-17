using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;

namespace NFe.Core.Cadastro.Emissor
{
    public interface IEmissorService
    {
        NotasFiscais.Emissor GetEmissor();
        EmitenteEntity GetEmitenteEntity();
        void Salvar(EmitenteEntity emitente);
    }
}