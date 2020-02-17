using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.UnitTests.ModoOnlineService
{
    class ConfiguracaoRepositoryFake : IConfiguracaoRepository
    {
        private ConfiguracaoEntity _configuracao;

        public ConfiguracaoRepositoryFake()
        {
            _configuracao = new ConfiguracaoEntity()
            {
                Id = 1,
                SerieNFCe = "001",
                SerieNFCeHom = "001",
                SerieNFe = "001",
                SerieNFeHom = "001",
                ProximoNumNFCe = "1",
                ProximoNumNFCeHom = "1",
                ProximoNumNFe = "1",
                ProximoNumNFeHom = "1",
                IsProducao = false,
            };
        }

        public void Excluir(ConfiguracaoEntity configuracao)
        {
            throw new NotImplementedException();
        }

        public ConfiguracaoEntity GetConfiguracao()
        {
            return _configuracao;
        }

        public int Salvar(ConfiguracaoEntity configuracao)
        {
            _configuracao = configuracao;
            return _configuracao.Id;
        }
    }
}
