using NFe.Core.Cadastro.Configuracoes;

namespace NFe.Core.Interfaces
{
    public interface IConfiguracaoRepository
    {
        int Salvar(ConfiguracaoEntity configuracao);
        void Excluir(ConfiguracaoEntity configuracao);
        ConfiguracaoEntity GetConfiguracao();
    }
}