using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Utils;

namespace NFe.Core.NotasFiscais.Services
{
    public class ImportadorXmlService
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventoService _eventoService;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly INotaInutilizadaService _notaInutilizadaService;

        public ImportadorXmlService(INotaFiscalRepository notaFiscalRepository,
            INotaInutilizadaService notaInutilizadaService, IEventoService eventoService)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _notaInutilizadaService = notaInutilizadaService;
            _eventoService = eventoService;
        }

        public async Task ImportarXmlAsync(string zipPath)
        {
            if (!File.Exists(zipPath))
                throw new FileNotFoundException("O arquivo específicado não existe no caminho informado.");

            var extractPath = Path.Combine(Path.GetTempPath(), "xml-nfe");

            if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                await ImportarXmlNotasFiscais(extractPath);
                await ImportarXmlEvento(extractPath);
                await ImportarXmlNotasFiscaisInutilizadas(extractPath);
            }
            catch (Exception e)
            {
                log.Error(e);
                throw new Exception("Ocorreu um erro ao tentar extrair o arquivo .zip informado. Mensagem: \n" +
                                    e.Message);
            }
            finally
            {
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            }
        }

        private async Task ImportarXmlNotasFiscais(string path)
        {
            var regex = new Regex(@"[0-9]{44}-nf[c|e][e]?");

            var files = Directory.EnumerateFiles(path, "*.xml").Where(f => regex.IsMatch(f));

            await Task.Run( () =>
            {
                foreach (var file in files)
                    try
                    {
                        var xml = File.ReadAllText(file);

                        var notaFiscal = _notaFiscalRepository.GetNotaFiscalFromNfeProcXml(xml);

                        var notaFiscalDb = _notaFiscalRepository.GetNotaFiscalByChave(notaFiscal.Identificacao.Chave);

                        if (notaFiscalDb != null) return;

                        var notaFiscalEntity = new NotaFiscalEntity();

                        if (notaFiscal.Destinatario != null && notaFiscal.Destinatario.Endereco != null)
                            notaFiscalEntity.UfDestinatario = notaFiscal.Destinatario.Endereco.UF;
                        else
                            notaFiscalEntity.UfDestinatario = notaFiscal.Emitente.Endereco.UF;

                        notaFiscalEntity.Destinatario = notaFiscal.Destinatario == null
                            ? "CONSUMIDOR NÃO IDENTIFICADO"
                            : notaFiscal.Destinatario.NomeRazao;
                        notaFiscalEntity.DocumentoDestinatario = notaFiscal.Destinatario == null
                            ? null
                            : notaFiscal.Destinatario.Documento;
                        notaFiscalEntity.Status = (int) notaFiscal.Identificacao.Status;
                        notaFiscalEntity.Chave = notaFiscal.Identificacao.Chave;
                        notaFiscalEntity.DataEmissao = notaFiscal.Identificacao.DataHoraEmissao;
                        notaFiscalEntity.Modelo = notaFiscal.Identificacao.Modelo == Modelo.Modelo55 ? "55" : "65";
                        notaFiscalEntity.Serie = notaFiscal.Identificacao.Serie.ToString();
                        notaFiscalEntity.TipoEmissao = notaFiscal.Identificacao.TipoEmissao.ToString();
                        notaFiscalEntity.ValorDesconto = notaFiscal.TotalNFe.IcmsTotal.ValorTotalDesconto;
                        notaFiscalEntity.ValorDespesas = notaFiscal.TotalNFe.IcmsTotal.ValorDespesasAcessorias;
                        notaFiscalEntity.ValorFrete = notaFiscal.TotalNFe.IcmsTotal.ValorTotalFrete;
                        notaFiscalEntity.ValorICMS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms;
                        notaFiscalEntity.ValorProdutos = notaFiscal.ValorTotalProdutos;
                        notaFiscalEntity.ValorSeguro = notaFiscal.TotalNFe.IcmsTotal.ValorTotalSeguro;
                        notaFiscalEntity.ValorTotal = notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe;
                        notaFiscalEntity.Numero = notaFiscal.Identificacao.Numero;
                        notaFiscalEntity.DataAutorizacao = DateTime.ParseExact(notaFiscal.DhAutorizacao,
                            "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
                        notaFiscalEntity.Protocolo = notaFiscal.ProtocoloAutorizacao;
                        notaFiscalEntity.XmlPath = GetFullPath(file);

                         _notaFiscalRepository.Salvar(notaFiscalEntity, xml);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
            });
        }

        private async Task ImportarXmlNotasFiscaisInutilizadas(string path)
        {
            var regex = new Regex(@"ID[0-9]{41}-procInutNFe");

            var files = Directory.EnumerateFiles(path, "*.xml").Where(f => regex.IsMatch(f));

            await Task.Run(() =>
            {
                foreach (var file in files)
                    try
                    {
                        var xml = File.ReadAllText(file);

                        var notaInutilizada = _notaInutilizadaService.GetNotaInutilizadaFromXml(xml);
                        var notaInutilizadaDb =
                            _notaInutilizadaService.GetNotaInutilizada(notaInutilizada.IdInutilizacao, false);

                        if (notaInutilizadaDb != null) return;

                        _notaInutilizadaService.Salvar(notaInutilizada, xml);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
            });
        }

        private async Task ImportarXmlEvento(string path)
        {
            var regex = new Regex(@"[0-9]{52}-procEventoNFe");

            var files = Directory.EnumerateFiles(path, "*.xml").Where(f => regex.IsMatch(f));

            await Task.Run( () =>
            {
                foreach (var file in files)
                    try
                    {
                        var xml = File.ReadAllText(file);
                        var fullPath = GetFullPath(file);

                        var eventoCancelamento =
                            _eventoService.GetEventoCancelamentoFromXml(xml, fullPath, _notaFiscalRepository);

                        var eventoDb = _eventoService.GetEventoPorNota(eventoCancelamento.NotaId, false);

                        if (eventoDb != null) return;

                        _eventoService.Salvar(eventoCancelamento);

                        var notaFiscalEntity =
                             _notaFiscalRepository.GetNotaFiscalById(eventoCancelamento.NotaId, false);
                        notaFiscalEntity.Status = (int) Status.CANCELADA;
                         _notaFiscalRepository.Salvar(notaFiscalEntity);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
            });
        }

        private void EscreverLogErro(Exception exception, string fileName)
        {
            var sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EmissorNFeDir");

            if (!Directory.Exists(sDirectory)) Directory.CreateDirectory(sDirectory);

            using (var stream = File.Create(Path.Combine(sDirectory, $"{fileName}-erro.txt")))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(exception.ToString());
                }
            }
        }

        /// <summary>
        ///     Método para retornar o caminho completo para ser salvo na máquina do cliente.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Caminho para ser amarzenado no banco de dados.</returns>
        private static string GetFullPath(string file)
        {
            var fileName = Path.GetFileName(file);
            if (fileName == null) throw new FileNotFoundException(file + " não existe!");

            var fullPath = Path.Combine(Global.XmlPath, fileName);
            return fullPath;

        }
    }
}