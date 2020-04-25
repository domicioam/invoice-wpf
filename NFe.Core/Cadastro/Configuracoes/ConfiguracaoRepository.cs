using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
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

        public int Salvar(ConfiguracaoEntity configuracao)
        {
            if (configuracao.Id == 0)
            {
                _context.Configuracao.Add(configuracao);
            }

            _context.SaveChanges();
            return configuracao.Id;
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
    }
}
