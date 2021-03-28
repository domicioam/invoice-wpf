using NFe.Core.Cadastro.Configuracoes;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IConsultaStatusServicoFacade
    {
        bool ExecutarConsultaStatus(ConfiguracaoEntity config, NFe.Core.NotaFiscal.Modelo modelo);
    }
}