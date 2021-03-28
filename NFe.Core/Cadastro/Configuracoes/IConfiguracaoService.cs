using System.Threading.Tasks;
using NFe.Core.Domain;

namespace NFe.Core.Cadastro.Configuracoes
{
    public interface IConfiguracaoService
    {
        ConfiguracaoEntity GetConfiguracao();
        Task<ConfiguracaoEntity> GetConfiguracaoAsync();
        string ObterProximoNumeroNotaFiscal(Modelo modelo);
        void Salvar(ConfiguracaoEntity configuracao);
        void SalvarPróximoNúmeroSérie(Modelo modelo, Ambiente ambiente);
    }
}