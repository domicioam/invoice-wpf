using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    // Domain Service
    public class NotaFiscalService
    {
        private IConfiguracaoRepository configuracaoRepository;

        public NotaFiscalService(IConfiguracaoRepository configuracaoRepository)
        {
            this.configuracaoRepository = configuracaoRepository;
        }

        public async Task<Numeracao> GerarNumeraçãoPróximaNotaFiscal(Modelo modelo)
        {
            var configuracao = await configuracaoRepository.GetConfiguracaoAsync();
            Numeracao numeracao = GerarNumeracao(configuracao, modelo);
            configuracao = AtualizaConfiguracaoNovaNumeracao(configuracao, modelo);
            configuracaoRepository.Salvar(configuracao);
            return numeracao;
        }

        private ConfiguracaoEntity AtualizaConfiguracaoNovaNumeracao(ConfiguracaoEntity configuracao, Modelo modelo)
        {
            if (modelo == Modelo.Modelo55)
                configuracao.ProximoNumNFe = (Convert.ToInt32(configuracao.ProximoNumNFe) + 1).ToString();
            else
                configuracao.ProximoNumNFCe = (Convert.ToInt32(configuracao.ProximoNumNFCe) + 1).ToString();

            return configuracao;
        }

        private Numeracao GerarNumeracao(ConfiguracaoEntity c, Modelo m)
        {
            switch (m)
            {
                case Modelo.Modelo55:
                    return new Numeracao(int.Parse(c.SerieNFe), int.Parse(c.ProximoNumNFe));
                case Modelo.Modelo65:
                    return new Numeracao(int.Parse(c.SerieNFCe), int.Parse(c.ProximoNumNFCe));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
