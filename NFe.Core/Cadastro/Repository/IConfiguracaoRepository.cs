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
        void SalvarPróximoNúmeroSérie(Modelo modelo, Ambiente ambiente);
        Task<ConfiguracaoEntity> GetConfiguracaoAsync();
        string ObterProximoNumeroNotaFiscal(Modelo modelo);
    }
}