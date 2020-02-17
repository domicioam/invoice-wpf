using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IConfiguracaoRepository
    {
        int Salvar(ConfiguracaoEntity configuracao);
        void Excluir(ConfiguracaoEntity configuracao);
        ConfiguracaoEntity GetConfiguracao();
    }
}