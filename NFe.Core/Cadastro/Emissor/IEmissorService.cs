using NFe.Core.Entitities;
using NFe.Core.NotaFiscal;

namespace NFe.Core.Cadastro.Emissor
{
    public interface IEmissorService
    {
        NotaFiscal.Emissor GetEmissor();
        EmitenteEntity GetEmitenteEntity();
        void Salvar(EmitenteEntity emitente);
    }
}