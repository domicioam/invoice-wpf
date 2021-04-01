using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.UnitTests.NotaFiscalService
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
                SerieNFe = "001",
                ProximoNumNFCe = "1",
                ProximoNumNFe = "1",
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

        public Task<ConfiguracaoEntity> GetConfiguracaoAsync()
        {
            throw new NotImplementedException();
        }

        public string ObterProximoNumeroNotaFiscal(Modelo modelo)
        {
            throw new NotImplementedException();
        }

        public int Salvar(ConfiguracaoEntity configuracao)
        {
            _configuracao = configuracao;
            return _configuracao.Id;
        }

        public void SalvarPróximoNúmeroSérie(Modelo modelo, Ambiente ambiente)
        {
            throw new NotImplementedException();
        }
    }
}
