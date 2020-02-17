using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private NFeContext _context;

        public EventoRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(EventoEntity evento)
        {
            _context.Evento.Add(evento);
            _context.SaveChanges();

            return evento.Id;
        }

        public void Excluir(EventoEntity evento)
        {
            _context.Evento.Remove(evento);
            _context.SaveChanges();
        }

        public List<EventoEntity> GetAll()
        {
            return _context.Evento.ToList();
        }

        public List<EventoEntity> GetEventosPorNotasId(IEnumerable<int> idsNotas)
        {
            return _context.Evento.Where(e => idsNotas.Contains(e.NotaId)).ToList();
        }

        public EventoEntity GetEventoPorNota(int idNota)
        {
            return _context.Evento.FirstOrDefault(e => e.NotaId == idNota);
        }

        public virtual void Insert(EventoEntity obj)
        {
            bool isEventoExiste = GetEventoPorNota(obj.NotaId) != null;

            if (!isEventoExiste)
            {
                _context.Evento.Add(obj);
            }
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }
    }
}
