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
    public class DestinatarioRepository : IDestinatarioRepository
    {
        private NFeContext _context;

        public DestinatarioRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(DestinatarioEntity destinatario)
        {
            if (destinatario.Id == 0)
            {
                _context.Entry(destinatario).State = EntityState.Added;
            }
            else
            {
                _context.Entry(destinatario).State = EntityState.Modified;
            }

            _context.SaveChanges();
            return destinatario.Id;
        }

        public List<DestinatarioEntity> GetAll()
        {
            return _context.Destinatario.ToList();
        }

        public Task<List<DestinatarioEntity>> GetAllAsync()
        {
            return _context.Destinatario.ToListAsync();
        }

        public DestinatarioEntity GetDestinatarioByID(int id)
        {
            return _context.Destinatario.FirstOrDefault(d => d.Id == id);
        }

        public void ExcluirDestinatario(int id)
        {
            var destinatario = _context.Destinatario.FirstOrDefault(d => d.Id == id);

            if (destinatario.Endereco != null)
            {
                _context.EnderecoDestinatario.Remove(destinatario.Endereco);
            }

            _context.Destinatario.Remove(destinatario);
            _context.SaveChanges();
        }
    }
}
