using System;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;

namespace NFe.Core.Cadastro.Configuracoes
{
    public class ConfiguracaoService : IConfiguracaoService
    {
        private ConfiguracaoEntity _configuracao;
        private IConfiguracaoRepository _configuracaoRepository;

        public ConfiguracaoService(IConfiguracaoRepository configuracaoRepository)
        {
            _configuracaoRepository = configuracaoRepository;
            _configuracao = _configuracaoRepository.GetConfiguracao();
        }

        public ConfiguracaoEntity GetConfiguracao()
        {
            return _configuracao = _configuracaoRepository.GetConfiguracao();
        }

        public Task<ConfiguracaoEntity> GetConfiguracaoAsync()
        {
            return Task.Run(() => { return _configuracao = _configuracaoRepository.GetConfiguracao(); });
        }

        public void Salvar(ConfiguracaoEntity configuracao)
        {
            var config = _configuracaoRepository.GetConfiguracao();

            if (config == null) config = new ConfiguracaoEntity();

            config.IsProducao = configuracao.IsProducao;

            config.CscHom = configuracao.CscHom;
            config.CscIdHom = configuracao.CscIdHom;
            config.EmailContabilidadeHom = configuracao.EmailContabilidadeHom;
            config.ProximoNumNFCeHom = configuracao.ProximoNumNFCeHom;
            config.ProximoNumNFeHom = configuracao.ProximoNumNFeHom;
            config.SerieNFCeHom = configuracao.SerieNFCeHom;
            config.SerieNFeHom = configuracao.SerieNFeHom;

            config.Csc = configuracao.Csc;
            config.CscId = configuracao.CscId;
            config.EmailContabilidade = configuracao.EmailContabilidade;
            config.ProximoNumNFCe = configuracao.ProximoNumNFCe;
            config.ProximoNumNFe = configuracao.ProximoNumNFe;
            config.SerieNFCe = configuracao.SerieNFCe;
            config.SerieNFe = configuracao.SerieNFe;

            config.IsContingencia = configuracao.IsContingencia;

            if (configuracao.DataHoraEntradaContingencia == new DateTime())
                config.DataHoraEntradaContingencia = DateTime.Now;
            else
                config.DataHoraEntradaContingencia = configuracao.DataHoraEntradaContingencia;

            config.JustificativaContingencia = configuracao.JustificativaContingencia;
            _configuracaoRepository.Salvar(config);
        }

        public void SalvarPróximoNúmeroSérie(Modelo modelo, Ambiente ambiente)
        {
            var config = GetConfiguracao();

            if (modelo == Modelo.Modelo55)
            {
                if (ambiente == Ambiente.Homologacao)
                    config.ProximoNumNFeHom = (Convert.ToInt32(config.ProximoNumNFeHom) + 1).ToString();
                else
                    config.ProximoNumNFe = (Convert.ToInt32(config.ProximoNumNFe) + 1).ToString();
            }
            else
            {
                if (ambiente == Ambiente.Homologacao)
                    config.ProximoNumNFCeHom = (Convert.ToInt32(config.ProximoNumNFCeHom) + 1).ToString();
                else
                    config.ProximoNumNFCe = (Convert.ToInt32(config.ProximoNumNFCe) + 1).ToString();
            }

            Salvar(config);
        }

        public string ObterProximoNumeroNotaFiscal(Modelo modelo)
        {
            var config = GetConfiguracao();

            if(modelo == Modelo.Modelo55)
            {
                return config.IsProducao ? config.ProximoNumNFe : config.ProximoNumNFeHom;

            }
            else
            {
                return config.IsProducao? config.ProximoNumNFCe: config.ProximoNumNFCeHom;
            }
        }
    }
}