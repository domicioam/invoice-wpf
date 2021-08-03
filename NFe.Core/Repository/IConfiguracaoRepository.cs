using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using System.Threading.Tasks;

namespace NFe.Core.Interfaces
{
    public interface IConfiguracaoRepository
    {
        int Salvar(ConfiguracaoEntity configuracao);
        void Excluir(ConfiguracaoEntity configuracao);
        ConfiguracaoEntity GetConfiguracao();
        Task<ConfiguracaoEntity> GetConfiguracaoAsync();
    }
}