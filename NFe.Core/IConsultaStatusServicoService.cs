using NFe.Core.Cadastro.Configuracoes;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IConsultaStatusServicoSefazService
    {
        bool ExecutarConsultaStatus(ConfiguracaoEntity config, NFe.Core.Domain.Modelo modelo);
    }
}