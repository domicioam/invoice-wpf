using System;
using System.Threading.Tasks;
using NFe.Core.Interfaces;
using NFe.Core.Domain;

namespace NFe.Core.Cadastro.Configuracoes
{
    public class ConfiguracaoService : IConfiguracaoService
    {
        private readonly IConfiguracaoRepository _configuracaoRepository;

        public ConfiguracaoService(IConfiguracaoRepository configuracaoRepository)
        {
            _configuracaoRepository = configuracaoRepository;
        }

        public ConfiguracaoEntity GetConfiguracao()
        {
            return _configuracaoRepository.GetConfiguracao();
        }

        public Task<ConfiguracaoEntity> GetConfiguracaoAsync()
        {
            return Task.Run(() => _configuracaoRepository.GetConfiguracao());
        }

        public void Salvar(ConfiguracaoEntity configuracao)
        {
            var config = _configuracaoRepository.GetConfiguracao() ?? new ConfiguracaoEntity();

            config.Csc = configuracao.Csc;
            config.CscId = configuracao.CscId;
            config.EmailContabilidade = configuracao.EmailContabilidade;
            config.ProximoNumNFCe = configuracao.ProximoNumNFCe;
            config.ProximoNumNFe = configuracao.ProximoNumNFe;
            config.SerieNFCe = configuracao.SerieNFCe;
            config.SerieNFe = configuracao.SerieNFe;

            config.IsContingencia = configuracao.IsContingencia;

            config.DataHoraEntradaContingencia = configuracao.DataHoraEntradaContingencia == new DateTime() ? DateTime.Now : configuracao.DataHoraEntradaContingencia;

            config.JustificativaContingencia = configuracao.JustificativaContingencia;
            _configuracaoRepository.Salvar(config);
        }

        public void SalvarPróximoNúmeroSérie(Modelo modelo, Ambiente ambiente)
        {
            var config = GetConfiguracao();

            if (modelo == Modelo.Modelo55)
            {
                config.ProximoNumNFe = (Convert.ToInt32(config.ProximoNumNFe) + 1).ToString();
            }
            else
            {
                config.ProximoNumNFCe = (Convert.ToInt32(config.ProximoNumNFCe) + 1).ToString();
            }

            Salvar(config);
        }

        public string ObterProximoNumeroNotaFiscal(Modelo modelo)
        {
            var config = GetConfiguracao();
            return modelo == Modelo.Modelo55 ? config.ProximoNumNFe : config.ProximoNumNFCe;
        }
    }
}