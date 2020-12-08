using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Destinatario
{
    public class DestinatarioRepository : IDestinatarioRepository
    {
        public int Salvar(DestinatarioEntity destinatario)
        {
            using (var context = new NFeContext())
            {
                if (destinatario.Id == 0)
                {
                    context.Entry(destinatario).State = EntityState.Added;
                }
                else
                {
                    context.Entry(destinatario).State = EntityState.Modified;
                    context.Entry(destinatario.Endereco).State = EntityState.Modified;
                }

                context.SaveChanges();
                return destinatario.Id;
            }
        }

        public List<DestinatarioEntity> GetAll()
        {
            using (var context = new NFeContext())
            {
                return context.Destinatario.Include(d => d.Endereco).ToList();
            }
        }

        public Task<List<DestinatarioEntity>> GetAllAsync()
        {
            using (var context = new NFeContext())
            {
                return context.Destinatario.Include(d => d.Endereco).ToListAsync();
            }
        }

        public DestinatarioEntity GetDestinatarioByID(int id)
        {
            using (var context = new NFeContext())
            {
                return context.Destinatario.Include(d => d.Endereco).FirstOrDefault(d => d.Id == id);
            }
        }

        public void ExcluirDestinatario(int id)
        {
            using (var context = new NFeContext())
            {
                var destinatario = context.Destinatario.FirstOrDefault(d => d.Id == id);

                if (destinatario.Endereco != null)
                {
                    context.EnderecoDestinatario.Remove(destinatario.Endereco);
                }

                context.Destinatario.Remove(destinatario);
                context.SaveChanges();
            }
        }
    }
}
