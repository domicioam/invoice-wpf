using System;
using System.Collections.Generic;
using System.Globalization;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno.Proc;

namespace NFe.Core.NotasFiscais.Services
{
    public class EventoService : IEventoService
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventoRepository _eventoRepository;

        public EventoService(IEventoRepository eventoRepository)
        {
            _eventoRepository = eventoRepository;
        }

        public virtual void Salvar(EventoEntity eventoEntity)
        {
            try
            {
                eventoEntity.XmlPath = XmlFileHelper.SaveXmlFile(eventoEntity, eventoEntity.Xml);
                _eventoRepository.Salvar(eventoEntity);
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
            var eventosCancelamentoNFe = _eventoRepository.GetEventosPorNotasId(idsNotas);

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
            var eventoCancelamentoNFe = _eventoRepository.GetEventoPorNota(idNota);

            if (isLoadXmlData) eventoCancelamentoNFe.LoadXml();

            return eventoCancelamentoNFe;
        }

        public EventoEntity GetEventoCancelamentoFromXml(string xml, string file,
            INotaFiscalRepository notaFiscalRepository)
        {
            var procEvento = (TProcEvento) XmlUtil.Deserialize<TProcEvento>(xml);
            var envInfEvento = procEvento.evento.infEvento;

            var notaFiscal =
                notaFiscalRepository.GetNotaFiscalByChave(envInfEvento.chNFe);

            if (notaFiscal == null)
                throw new ArgumentException("Nota fiscal relacionada ao evento não existe na base de dados.");

            var evento = new EventoEntity
            {
                TipoEvento = envInfEvento.tpEvento,
                DataEvento = DateTime.ParseExact(envInfEvento.dhEvento, "yyyy-MM-ddTHH:mm:sszzz",
                    CultureInfo.InvariantCulture),
                ChaveIdEvento = envInfEvento.Id,
                MotivoCancelamento = envInfEvento.detEvento.Any[2].InnerText,
                ProtocoloCancelamento = envInfEvento.detEvento.Any[1].InnerText,
                NotaId = notaFiscal.Id
            };

            return evento;
        }
    }
}