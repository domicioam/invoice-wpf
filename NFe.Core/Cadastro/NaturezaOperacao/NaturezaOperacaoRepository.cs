
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
    public class NaturezaOperacaoRepository : INaturezaOperacaoRepository
    {
        private NFeContext _context;

        public NaturezaOperacaoRepository()
        {
            _context = new NFeContext();
        }

        public NaturezaOperacaoEntity GetNaturezaOperacaoPorCfop(string cfop)
        {
            var cfopEntity = _context.Cfop.FirstOrDefault(c => c.Cfop == cfop.Replace(" ", ""));
            return _context.NaturezaOperacao.FirstOrDefault(c => c.Id == cfopEntity.NaturezaOperacaoId);
        }

        public List<NaturezaOperacaoEntity> GetAll()
        {
            return _context.NaturezaOperacao.ToList();
        }
    }
}
