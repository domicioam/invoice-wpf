using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils.PDF;
using NFe.Core.Utils.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;


//TODO: Adicionar try catch para hd cheio ou para quando não é possível criar o diretório e etc

namespace NFe.Core.Utils.Zip
{
    public class GeradorZip
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private INotaFiscalRepository _notaFiscalRepository;
        private IEventoService _eventoService;
        private INotaInutilizadaService _notaInutilizadaService;
        private GeradorPDF _geradorPdf;

        public GeradorZip(IEventoService eventoService, INotaInutilizadaService notaInutilizadaService,
            GeradorPDF geradorPdf, INotaFiscalRepository notaFiscalRepository)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _eventoService = eventoService;
            _notaInutilizadaService = notaInutilizadaService;
            _geradorPdf = geradorPdf;
        }

        public Task<string> GerarZipEnvioContabilidadeAsync(DateTime periodo)
        {
            return Task.Run(() =>
            {
                var notasNoPeriodo = _notaFiscalRepository.GetNotasFiscaisPorMesAno(periodo, true);

                var nfeNoPeriodo =
                    notasNoPeriodo
                       .Where(n => n.Modelo.Equals("55"));

                var nfeXMLs = nfeNoPeriodo.Select(n => new NotaXml(n.Chave, n.LoadXml()));

                var eventoNfeXMLs =
                 _eventoService.GetEventosPorNotasId(nfeNoPeriodo.Select(n => n.Id), true)
                     .Select(e => new EventoCancelamentoXml(e.ChaveIdEvento, e.Xml));

                var nfceNoPeriodo =
                   notasNoPeriodo
                       .Where(n => n.Modelo.Equals("65"));

                var nfceXMLs =
                    nfceNoPeriodo
                       .Select(n => new NotaXml(n.Chave, n.LoadXml()));

                var eventoNfceXMLs =
                    _eventoService.GetEventosPorNotasId(nfceNoPeriodo.Select(n => n.Id), true)
                       .Select(e => new EventoCancelamentoXml(e.ChaveIdEvento, e.Xml));

                var notasInutilizadas = _notaInutilizadaService.GetNotasFiscaisPorMesAno(periodo);

                string startPath = Path.Combine(Path.GetTempPath(), "EmissorNFe");

                try
                {
                    if (!Directory.Exists(startPath))
                    {
                        Directory.CreateDirectory(startPath);
                    }
                    else
                    {
                        Directory.Delete(startPath, true);
                        Directory.CreateDirectory(startPath);
                    }

                    string nfeDir = Path.Combine(startPath, "NFe");
                    string nfceDir = Path.Combine(startPath, "NFCe");

                    string zipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Notas Fiscais " + periodo.ToString("MM_yyyy") + ".zip");

                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }

                    _geradorPdf.GerarRelatorioGerencial(notasNoPeriodo, notasInutilizadas, periodo, startPath);
                    //Gerar arquivos de notas inutilizadas e adicioná-las ao relatório

                    if (GravarXmlsNfe(nfeXMLs, eventoNfeXMLs, nfeDir)
                        && GravarXmlsNfce(nfceXMLs, eventoNfceXMLs, nfceDir)
                        && GerarXmlsNotasInutilizadas(notasInutilizadas, startPath))
                    {
                        ZipFile.CreateFromDirectory(startPath, zipPath);
                    }

                    return zipPath;
                }
                catch (Exception e)
                {
                    log.Error(e);
                    throw;
                }
                finally
                {
                    Directory.Delete(startPath, true);
                }
            });
        }

        private bool GerarXmlsNotasInutilizadas(List<NotaInutilizadaTO> notasInutilizadas, string startPath)
        {
            try
            {
                string inutDir = Path.Combine(startPath, "Inutilizadas");

                if (!Directory.Exists(inutDir))
                {
                    Directory.CreateDirectory(inutDir);
                }

                string nfeDir = Path.Combine(inutDir, "NFe");

                if (!Directory.Exists(nfeDir))
                {
                    Directory.CreateDirectory(nfeDir);
                }

                string nfceDir = Path.Combine(inutDir, "NFCe");

                if (!Directory.Exists(nfceDir))
                {
                    Directory.CreateDirectory(nfceDir);
                }

                notasInutilizadas
                    .AsParallel()
                    .ForAll(n =>
                    {
                        string path = string.Empty;

                        switch (n.Modelo)
                        {
                            case 55:
                                path = Path.Combine(nfeDir, n.IdInutilizacao + "-procInutNFe.xml");
                                break;

                            case 65:
                                path = Path.Combine(nfceDir, n.IdInutilizacao + "-procInutNFe.xml");
                                break;
                            default:
                                throw new NotSupportedException("Modelo não suportado.");
                        }

                        using (FileStream stream = File.Create(path))
                        {
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.WriteLine(n.LoadXml());
                            }
                        }
                    });

                return true;
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }
        }

        private bool GravarXmlsNfe(IEnumerable<NotaXml> xmlNfeEnviadas, IEnumerable<EventoCancelamentoXml> xmlNfeEventoCancelamento, string nfeDir)
        {
            if (!Directory.Exists(nfeDir))
            {
                Directory.CreateDirectory(nfeDir);
            }

            foreach (var nfe in xmlNfeEnviadas)
            {
                using (FileStream stream = File.Create(Path.Combine(nfeDir, nfe.Chave + "-nfe.xml")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(nfe.Xml);
                    }
                }
            }

            foreach (var evento in xmlNfeEventoCancelamento)
            {
                using (FileStream stream = File.Create(Path.Combine(nfeDir, evento.Id + "-procEventoNFe.xml")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(evento.Xml);
                    }
                }
            }

            return true;
        }

        private bool GravarXmlsNfce(IEnumerable<NotaXml> xmlNfceEnviadas, IEnumerable<EventoCancelamentoXml> xmlNfceEventoCancelamento, string nfceDir)
        {
            if (!Directory.Exists(nfceDir))
            {
                Directory.CreateDirectory(nfceDir);
            }

            foreach (var p in xmlNfceEnviadas)
            {
                using (FileStream stream = File.Create(Path.Combine(nfceDir, p.Chave + "-nfce.xml")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(p.Xml);
                    }
                }
            }

            foreach (var evento in xmlNfceEventoCancelamento)
            {
                using (FileStream stream = File.Create(Path.Combine(nfceDir, evento.Id + "-procEventoNFe.xml")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(evento.Xml);
                    }
                }
            }

            return true;
        }
    }

    public class NotaXml
    {
        public NotaXml(string chave, string xml)
        {
            Chave = chave;
            Xml = xml;
        }

        public string Chave { get; set; }
        public string Xml { get; set; }
    }

    public class EventoCancelamentoXml
    {
        public EventoCancelamentoXml(string id, string xml)
        {
            Id = id;
            Xml = xml;
        }

        public string Id { get; set; }
        public string Xml { get; set; }
    }
}
