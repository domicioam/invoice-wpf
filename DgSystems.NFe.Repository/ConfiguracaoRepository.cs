using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Domain;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Configuracoes
{
    public class ConfiguracaoRepository : IConfiguracaoRepository
    {
        private NFeContext _context;

        public ConfiguracaoRepository()
        {
            _context = new NFeContext();
        }

        public void Excluir(ConfiguracaoEntity configuracao)
        {
            _context.Configuracao.Remove(configuracao);
            _context.SaveChanges();
        }

        public ConfiguracaoEntity GetConfiguracao()
        {
            return _context.Configuracao.FirstOrDefault();
        }

        public Task<ConfiguracaoEntity> GetConfiguracaoAsync()
        {
            return Task.Run(() => GetConfiguracao());
        }

        public int Salvar(ConfiguracaoEntity configuracao)
        {
            var config = GetConfiguracao() ?? new ConfiguracaoEntity();

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

            if (configuracao.Id == 0)
            {
                _context.Configuracao.Add(configuracao);
            }

            _context.SaveChanges();
            return configuracao.Id;
        }
    }
}
