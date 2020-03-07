using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
    public class GrupoImpostosRepository : IGrupoImpostosRepository
    {
        private NFeContext _context;

        public GrupoImpostosRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(GrupoImpostos grupoImpostos)
        {
            var entity = _context.GrupoImpostos.Where(g => g.Id == grupoImpostos.Id).FirstOrDefault();
            if (grupoImpostos.Id == 0)
            {
                _context.Entry(grupoImpostos).State = EntityState.Added;
            }
            else
            {
                _context.Entry(entity).CurrentValues.SetValues(grupoImpostos);
            }
            _context.SaveChanges();

            return grupoImpostos.Id;
        }

        public void Excluir(GrupoImpostos grupoImpostos)
        {
            _context.GrupoImpostos.Remove(grupoImpostos);
            _context.SaveChanges();
        }

        public List<GrupoImpostos> GetAll()
        {
            return _context.GrupoImpostos.ToList();
        }

        public GrupoImpostos GetById(int id)
        {
            return _context.GrupoImpostos.Where(i => i.Id == id).FirstOrDefault();
        }
    }
}
