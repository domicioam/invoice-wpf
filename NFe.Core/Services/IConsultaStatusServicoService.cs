using NFe.Core.Cadastro.Configuracoes;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IConsultaStatusServicoSefazService
    {
        bool ExecutarConsultaStatus(NFe.Core.Domain.Modelo modelo, string sefazEnvironment);
    }
}