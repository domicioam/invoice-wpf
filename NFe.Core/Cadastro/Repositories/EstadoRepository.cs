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
    public class EstadoRepository : IEstadoRepository
    {
        private NFeContext _context;

        public EstadoRepository()
        {
            _context = new NFeContext();
        }

        public List<EstadoEntity> GetEstados()
        {
            return _context.Estado.ToList();
        }
    }
}
