using NFe.Core.Entitities;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IConsultaStatusServicoService
    {
        bool ExecutarConsultaStatus(ConfiguracaoEntity config, Modelo modelo);
    }
}