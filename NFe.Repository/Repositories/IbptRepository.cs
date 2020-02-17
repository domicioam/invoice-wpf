using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
    public class IbptRepository : IIbptRepository
    {
        private NFeContext _context;

        public IbptRepository()
        {
            _context = new NFeContext();
        }

        public IbptEntity GetValoresPorNCM(string ncmProduto)
        {
            return _context.Ibpt.Where(i => i.NCM == ncmProduto).FirstOrDefault();
        }

        public List<IbptEntity> GetValoresPorListaNCM(IEnumerable<string> ncmList)
        {
            return _context.Ibpt.Where(i => ncmList.Contains(i.NCM)).ToList();
        }
    }
}
