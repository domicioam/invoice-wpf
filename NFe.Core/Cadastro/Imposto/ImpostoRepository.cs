using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DgSystems.NFe.Core.Cadastro;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Imposto
{
    public class GrupoImpostosRepository : IGrupoImpostosRepository
    {
        public int Salvar(GrupoImpostos grupoImpostos)
        {
            using (var context = new NFeContext())
            {
                var grupoImpostosExistente = context.GrupoImpostos.FirstOrDefault(g => g.Id == grupoImpostos.Id);
                if (grupoImpostos.Id == 0)
                {
                    context.Entry(grupoImpostos).State = EntityState.Added;
                }
                else
                {
                    context.Entry(grupoImpostosExistente).CurrentValues.SetValues(grupoImpostos);

                    // add new items

                    var comparer = new ImpostoIdComparer();
                    var impostosParaAdicionar = grupoImpostos.Impostos.Except(grupoImpostosExistente.Impostos, comparer)
                        .ToList();

                    foreach (var imposto in impostosParaAdicionar)
                    {
                        grupoImpostosExistente.Impostos.Add(imposto);
                    }

                    // remove items

                    var impostosParaRemover = grupoImpostosExistente.Impostos.Except(grupoImpostos.Impostos, comparer)
                        .ToList();

                    foreach (var imposto in impostosParaRemover)
                    {
                        context.Entry(imposto).State = EntityState.Deleted;
                    }
                }

                context.SaveChanges();

                return grupoImpostos.Id;
            }
        }

        public void Excluir(GrupoImpostos grupoImpostos)
        {
            using (var context = new NFeContext())
            {
                context.Entry(grupoImpostos).State = EntityState.Modified;
                context.GrupoImpostos.Remove(grupoImpostos);
                
                context.SaveChanges();
            }
        }

        public List<GrupoImpostos> GetAll()
        {
            using (var context = new NFeContext())
            {
                return context.GrupoImpostos.Include(g => g.Impostos).ToList();
            }
        }

        public GrupoImpostos GetById(int id)
        {
            using (var context = new NFeContext())
            {
                return context.GrupoImpostos.Include(g => g.Impostos).FirstOrDefault(i => i.Id == id);
            }
        }
    }
}
