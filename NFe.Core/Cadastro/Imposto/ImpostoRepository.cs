using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Cadastro.Imposto;
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
            var grupoImpostosExistente = _context.GrupoImpostos.Where(g => g.Id == grupoImpostos.Id).FirstOrDefault();
            if (grupoImpostos.Id == 0)
            {
                _context.Entry(grupoImpostos).State = EntityState.Added;
            }
            else
            {
                _context.Entry(grupoImpostosExistente).CurrentValues.SetValues(grupoImpostos);

                // add new items

                ImpostoIdComparer comparer = new ImpostoIdComparer();
                var impostosParaAdicionar = grupoImpostos.Impostos.Except(grupoImpostosExistente.Impostos, comparer).ToList();

                foreach (var imposto in impostosParaAdicionar)
                {
                    grupoImpostosExistente.Impostos.Add(imposto);
                }

                // remove items

                var impostosParaRemover = grupoImpostosExistente.Impostos.Except(grupoImpostos.Impostos, comparer).ToList();

                foreach (var imposto in impostosParaRemover)
                {
                    _context.Entry(imposto).State = EntityState.Deleted;
                }
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
