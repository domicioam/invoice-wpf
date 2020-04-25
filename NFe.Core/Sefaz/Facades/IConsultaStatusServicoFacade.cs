using NFe.Core.Entitities;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IConsultaStatusServicoFacade
    {
        bool ExecutarConsultaStatus(ConfiguracaoEntity config, Modelo modelo);
    }
}