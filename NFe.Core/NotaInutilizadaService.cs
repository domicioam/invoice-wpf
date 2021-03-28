using System;
using System.Collections.Generic;
using System.Globalization;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeInutilizacao2.Retorno.ProcInut;

namespace NFe.Core.NotasFiscais.Services
{
    public class NotaInutilizadaService : INotaInutilizadaService
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly INotaInutilizadaRepository _notaInutilizadaRepository;

        public NotaInutilizadaService(INotaInutilizadaRepository notaInutilizadaRepository)
        {
            _notaInutilizadaRepository = notaInutilizadaRepository;
        }

        public List<NotaInutilizadaTO> GetNotasFiscaisPorMesAno(DateTime periodo)
        {
            var notasInutilizadasDB = _notaInutilizadaRepository.GetNotasFiscaisPorMesAno(periodo);

            var notasInutilizadas = new List<NotaInutilizadaTO>();

            foreach (var notaInutDb in notasInutilizadasDB) notasInutilizadas.Add((NotaInutilizadaTO) notaInutDb);

            return notasInutilizadas;
        }

        public virtual void Salvar(NotaInutilizadaTO notaInutilizada, string xml)
        {
            try
            {
                notaInutilizada.XmlPath = XmlFileHelper.SaveXmlFile(notaInutilizada, xml);
                _notaInutilizadaRepository.Salvar((NotaInutilizadaEntity) notaInutilizada);
            }
            catch (Exception e)
            {
                log.Error(e);
                try
                {
                    XmlFileHelper.DeleteXmlFile(notaInutilizada);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    throw new Exception("Não foi possível remover o xml de nota fiscal: " + e.Message);
                }
            }
        }



        public NotaInutilizadaTO GetNotaInutilizada(string idInutilizacao, bool isLoadXmlData)
        {
            var notaInutilizada =
                (NotaInutilizadaTO) _notaInutilizadaRepository.GetNotaInutilizada(idInutilizacao);

            if (isLoadXmlData && notaInutilizada != null) notaInutilizada.LoadXml();

            return notaInutilizada;
        }

        public NotaInutilizadaTO GetNotaInutilizadaFromXml(string xml)
        {
            var procInutNFe = (TProcInutNFe) XmlUtil.Deserialize<TProcInutNFe>(xml);
            var envInfInut = procInutNFe.inutNFe.infInut;
            var retInfInut = procInutNFe.retInutNFe.infInut;

            var notaInutilizada = new NotaInutilizadaTO();
            notaInutilizada.Serie = envInfInut.serie;
            notaInutilizada.Numero = envInfInut.nNFFin;
            notaInutilizada.Modelo = envInfInut.mod == TMod.Item55 ? 55 : 65;
            notaInutilizada.DataInutilizacao = DateTime.ParseExact(retInfInut.dhRecbto, "yyyy-MM-ddTHH:mm:sszzz",
                CultureInfo.InvariantCulture);
            notaInutilizada.IdInutilizacao = envInfInut.Id;
            notaInutilizada.Protocolo = retInfInut.nProt;
            notaInutilizada.Motivo = envInfInut.xJust;

            return notaInutilizada;
        }
    }
}