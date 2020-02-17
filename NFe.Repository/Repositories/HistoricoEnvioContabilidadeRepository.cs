using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
   public class HistoricoEnvioContabilidadeRepository : IHistoricoEnvioContabilidadeRepository
   {
      private NFeContext _context;

      public HistoricoEnvioContabilidadeRepository()
      {
         _context = new NFeContext();
      }

      public int Salvar(HistoricoEnvioContabilidade emitente)
      {
         if (emitente.Id == 0)
         {
            _context.HistoricoEnvioContabilidade.Add(emitente);
         }

         _context.SaveChanges();
         return emitente.Id;
      }

      public Task<List<HistoricoEnvioContabilidade>> GetAllAsync()
      {
         return Task.Run(() =>
         {
            return _context.HistoricoEnvioContabilidade.ToList();
         });
      }

      public Task<int> GetHistoricoByPeriodoAsync(DateTime periodo)
      {
         return Task.Run(() =>
         {
            var dataInicial = DateTime.Now.AddMonths(-1);
            return _context.HistoricoEnvioContabilidade.Count(h => h.DataEnvio >= dataInicial);
         });
      }
   }
}
