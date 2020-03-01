using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private NFeContext _context;

        public ProdutoRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(ProdutoEntity produto)
        {
            if (produto.Id == 0)
            {
                _context.Entry(produto).State = EntityState.Added;
            }
            else
            {
                var local = _context.Set<ProdutoEntity>()
                    .Local
                    .FirstOrDefault(e => e.Id == produto.Id);

                _context.Entry(local).State = EntityState.Detached;
                _context.Entry(produto).State = EntityState.Modified;
            }

            _context.SaveChanges();
            return produto.Id;
        }

        public void Excluir(ProdutoEntity produto)
        {
            _context.Produto.Remove(produto);
            _context.SaveChanges();
        }

        public List<ProdutoEntity> GetAll()
        {
            return _context.Produto.ToList();
        }

        public ProdutoEntity GetProdutoByNcm(string ncm)
        {
            return _context.Produto.FirstOrDefault(p => p.NCM.Equals(ncm));
        }

        public ProdutoEntity GetByCodigo(string codigo)
        {
            return _context.Produto.FirstOrDefault(p => p.Codigo.Equals(codigo));
        }
    }
}
