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
    public class MunicipioRepository : IMunicipioRepository
    {
        private NFeContext _context;

        public MunicipioRepository()
        {
            _context = new NFeContext();
        }

        public List<MunicipioEntity> GetMunicipioByUf(string uf)
        {
            return _context.Municipio.Where(m => m.Uf.Equals(uf)).ToList();
        }

    }
}
