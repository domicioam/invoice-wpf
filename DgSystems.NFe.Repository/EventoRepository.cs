using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno.Proc;

namespace NFe.Repository.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private NFeContext _context;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EventoRepository()
        {
            _context = new NFeContext();
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

        public virtual void Salvar(EventoEntity eventoEntity)
        {
            try
            {
                eventoEntity.XmlPath = XmlFileHelper.SaveXmlFile(eventoEntity, eventoEntity.Xml);
                _context.Evento.Add(eventoEntity);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                log.Error(e);
                try
                {
                    XmlFileHelper.DeleteXmlFile(eventoEntity);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    throw new Exception("Não foi possível remover o xml de evento: " + e.Message);
                }
            }
        }

        public List<EventoEntity> GetEventosPorNotasId(IEnumerable<int> idsNotas, bool isLoadXmlData)
        {
            var eventosCancelamentoNFe = GetEventosPorNotasId(idsNotas);

            var eventos = new List<EventoEntity>();

            foreach (var eventoCancelamento in eventosCancelamentoNFe)
            {
                var eventoCancelamentoNFe = eventoCancelamento;

                if (isLoadXmlData) eventoCancelamentoNFe.LoadXml();

                eventos.Add(eventoCancelamentoNFe);
            }

            return eventos;
        }

        public EventoEntity GetEventoPorNota(int idNota, bool isLoadXmlData)
        {
            var eventoCancelamentoNFe = GetEventoPorNota(idNota);

            if (isLoadXmlData) eventoCancelamentoNFe.LoadXml();

            return eventoCancelamentoNFe;
        }

        public EventoEntity GetEventoCancelamentoFromXml(string xml, string file,
            INotaFiscalRepository notaFiscalRepository)
        {
            var procEvento = (TProcEvento)XmlUtil.Deserialize<TProcEvento>(xml);
            var envInfEvento = procEvento.evento.infEvento;

            var notaFiscal =
                notaFiscalRepository.GetNotaFiscalByChave(envInfEvento.chNFe);

            if (notaFiscal == null)
                throw new ArgumentException("Nota fiscal relacionada ao evento não existe na base de dados.");

            return new EventoEntity
            {
                TipoEvento = envInfEvento.tpEvento,
                DataEvento = DateTime.ParseExact(envInfEvento.dhEvento, "yyyy-MM-ddTHH:mm:sszzz",
                    CultureInfo.InvariantCulture),
                ChaveIdEvento = envInfEvento.Id,
                MotivoCancelamento = envInfEvento.detEvento.Any[2].InnerText,
                ProtocoloCancelamento = envInfEvento.detEvento.Any[1].InnerText,
                NotaId = notaFiscal.Id
            };
        }
    }
}
