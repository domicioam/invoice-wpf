using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeInutilizacao2.Retorno.ProcInut;


namespace NFe.Repository.Repositories
{
    public class NotaInutilizadaRepository : INotaInutilizadaRepository
    {
        private NFeContext _context;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public NotaInutilizadaRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(NotaInutilizadaEntity notaInutilizada)
        {
            _context.NotaInutilizada.Add(notaInutilizada);
            _context.SaveChanges();
            return notaInutilizada.Id;
        }

        public NotaInutilizadaEntity GetNotaInutilizada(string idInutilizacao)
        {
            return _context.NotaInutilizada.FirstOrDefault(n => n.IdInutilizacao == idInutilizacao);
        }

        public virtual void Insert(NotaInutilizadaEntity novaNotaInutilizada)
        {
            bool isNotaInutilizadaJáExiste = GetNotaInutilizada(novaNotaInutilizada.IdInutilizacao) != null;

            if (!isNotaInutilizadaJáExiste)
            {
                _context.NotaInutilizada.Add(novaNotaInutilizada);
            }
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }

        public List<NotaInutilizada> GetNotasFiscaisPorMesAno(DateTime periodo)
        {
            var notasInutilizadasDB = _context.NotaInutilizada.Where(n => n.DataInutilizacao.Month.Equals(periodo.Month) && n.DataInutilizacao.Year == periodo.Year);

            var notasInutilizadas = new List<NotaInutilizada>();

            foreach (var notaInutDb in notasInutilizadasDB) notasInutilizadas.Add((NotaInutilizada)notaInutDb);

            return notasInutilizadas;
        }

        public virtual void Salvar(NotaInutilizada notaInutilizada, string xml)
        {
            try
            {
                notaInutilizada.XmlPath = XmlFileHelper.SaveXmlFile(notaInutilizada, xml);
                Salvar((NotaInutilizadaEntity)notaInutilizada);
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



        public NotaInutilizada GetNotaInutilizada(string idInutilizacao, bool isLoadXmlData)
        {
            var notaInutilizada =
                (NotaInutilizada)GetNotaInutilizada(idInutilizacao);

            if (isLoadXmlData && notaInutilizada != null) notaInutilizada.LoadXml();

            return notaInutilizada;
        }

        public NotaInutilizada GetNotaInutilizadaFromXml(string xml)
        {
            var procInutNFe = (TProcInutNFe)XmlUtil.Deserialize<TProcInutNFe>(xml);
            var envInfInut = procInutNFe.inutNFe.infInut;
            var retInfInut = procInutNFe.retInutNFe.infInut;

            var notaInutilizada = new NotaInutilizada();
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
