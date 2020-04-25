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
    public class TransportadoraRepository : ITransportadoraRepository
    {
        private NFeContext _context;

        public TransportadoraRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(TransportadoraEntity transportadora)
        {
            _context.Transportadora.Add(transportadora);
            _context.SaveChanges();

            return transportadora.Id;
        }

        public void Excluir(int id)
        {
            var transportadora = _context.Transportadora.First(t => t.Id == id);
            _context.Transportadora.Remove(transportadora);
            _context.SaveChanges();
        }

        public List<TransportadoraEntity> GetAll()
        {
            return _context.Transportadora.ToList();
        }
    }
}
