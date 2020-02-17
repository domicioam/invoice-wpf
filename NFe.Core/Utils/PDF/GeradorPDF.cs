using Microsoft.Reporting.WinForms;
using NFe.Core;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Zen.Barcode;
using static QRCoder.PayloadGenerator;
using System.Drawing;
using System.Xml;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.Utils.PDF.RelatorioGerencial;
using NotaFiscal = NFe.Core.NotasFiscais.NotaFiscal;

namespace NFe.Core.Utils.PDF
{
    public class GeradorPDF
    {
        private IEmitenteRepository _emitenteRepository;
        private IEventoRepository _eventoRepository;

        public GeradorPDF(IEmitenteRepository emitenteRepository, IEventoRepository eventoRepository)
        {
            _emitenteRepository = emitenteRepository;
            _eventoRepository = eventoRepository;
        }

        public void GerarRelatorioGerencial(List<NotaFiscalEntity> notasPeriodo, List<NotaInutilizadaTO> notasInutilizadas, DateTime mesAno, string startPath)
        {
            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            var emitenteEntity = _emitenteRepository.GetEmitente();
            var emitente = new RelatorioGerencial.Emitente()
            {
                RazaoSocial = emitenteEntity.RazaoSocial,
                CNPJ = emitenteEntity.CNPJ
            };

            IEnumerable<RelatorioGerencial.NotaFiscal> nfceAutorizadas = notasPeriodo
                .Where(n => n.Modelo.Equals("65") && n.Status == (int)Status.ENVIADA)
                .Select(nota => new RelatorioGerencial.NotaFiscal()
                {
                    ValorDesconto = nota.ValorDesconto,
                    DataAutorizacao = nota.DataAutorizacao,
                    DataEmissao = nota.DataEmissao,
                    Modelo = nota.Modelo,
                    Numero = nota.Numero,
                    Protocolo = nota.Protocolo,
                    Serie = nota.Serie,
                    TipoEmissao = nota.TipoEmissao,
                    ValorDespesas = nota.ValorDespesas,
                    ValorFrete = nota.ValorFrete,
                    ValorICMS = nota.ValorICMS,
                    ValorICMSST = nota.ValorICMSST,
                    ValorIPI = nota.ValorIPI,
                    ValorISS = nota.ValorISS,
                    ValorProdutos = nota.ValorProdutos,
                    ValorSeguro = nota.ValorSeguro,
                    ValorServicos = nota.ValorServicos,
                    ValorTotal = nota.ValorTotal
                });

            IEnumerable<RelatorioGerencial.NotaFiscal> nfeAutorizadas = notasPeriodo.Where(n => n.Modelo.Equals("55") && n.Status == (int)Status.ENVIADA).Select(nota => new RelatorioGerencial.NotaFiscal()
            {
                ValorDesconto = nota.ValorDesconto,
                DataAutorizacao = nota.DataAutorizacao,
                DataEmissao = nota.DataEmissao,
                Modelo = nota.Modelo,
                Numero = nota.Numero,
                Protocolo = nota.Protocolo,
                Serie = nota.Serie,
                TipoEmissao = nota.TipoEmissao,
                ValorDespesas = nota.ValorDespesas,
                ValorFrete = nota.ValorFrete,
                ValorICMS = nota.ValorICMS,
                ValorICMSST = nota.ValorICMSST,
                ValorIPI = nota.ValorIPI,
                ValorISS = nota.ValorISS,
                ValorProdutos = nota.ValorProdutos,
                ValorSeguro = nota.ValorSeguro,
                ValorServicos = nota.ValorServicos,
                ValorTotal = nota.ValorTotal
            });

            IEnumerable<RelatorioGerencial.NotaFiscal> nfceCanceladas = notasPeriodo.Where(n => n.Modelo.Equals("65") && n.Status == (int)Status.CANCELADA).Select(nota => new RelatorioGerencial.NotaFiscal()
            {
                Id = nota.Id,
                ValorDesconto = nota.ValorDesconto,
                DataAutorizacao = nota.DataAutorizacao,
                DataEmissao = nota.DataEmissao,
                Modelo = nota.Modelo,
                Numero = nota.Numero,
                Protocolo = nota.Protocolo,
                Serie = nota.Serie,
                TipoEmissao = nota.TipoEmissao,
                ValorDespesas = nota.ValorDespesas,
                ValorFrete = nota.ValorFrete,
                ValorICMS = nota.ValorICMS,
                ValorICMSST = nota.ValorICMSST,
                ValorIPI = nota.ValorIPI,
                ValorISS = nota.ValorISS,
                ValorProdutos = nota.ValorProdutos,
                ValorSeguro = nota.ValorSeguro,
                ValorServicos = nota.ValorServicos,
                ValorTotal = nota.ValorTotal
            });
            IEnumerable<RelatorioGerencial.NotaFiscal> nfeCanceladas = notasPeriodo.Where(n => n.Modelo.Equals("55") && n.Status == (int)Status.CANCELADA).Select(nota => new RelatorioGerencial.NotaFiscal()
            {
                Id = nota.Id,
                ValorDesconto = nota.ValorDesconto,
                DataAutorizacao = nota.DataAutorizacao,
                DataEmissao = nota.DataEmissao,
                Modelo = nota.Modelo,
                Numero = nota.Numero,
                Protocolo = nota.Protocolo,
                Serie = nota.Serie,
                TipoEmissao = nota.TipoEmissao,
                ValorDespesas = nota.ValorDespesas,
                ValorFrete = nota.ValorFrete,
                ValorICMS = nota.ValorICMS,
                ValorICMSST = nota.ValorICMSST,
                ValorIPI = nota.ValorIPI,
                ValorISS = nota.ValorISS,
                ValorProdutos = nota.ValorProdutos,
                ValorSeguro = nota.ValorSeguro,
                ValorServicos = nota.ValorServicos,
                ValorTotal = nota.ValorTotal
            });

            var eventoCancelamentoNFCe = _eventoRepository.GetEventosPorNotasId(nfceCanceladas.Select(n => n.Id));
            var eventoCancelamentoNFe = _eventoRepository.GetEventosPorNotasId(nfeCanceladas.Select(n => n.Id));

            List<RelatorioGerencial.NotaInutilizada> nfceInutilizadas = notasInutilizadas.Where(n => n.Modelo.Equals(65)).Select(
                nota => new RelatorioGerencial.NotaInutilizada()
                {
                    Serie = nota.Serie,
                    Numero = nota.Numero,
                    Protocolo = nota.Protocolo,
                    DataInutilizacao = nota.DataInutilizacao,
                    Motivo = nota.Motivo
                }
                ).ToList();

            List<RelatorioGerencial.NotaInutilizada> nfeInutilizadas = notasInutilizadas.Where(n => n.Modelo.Equals(55)).Select(
                nota => new RelatorioGerencial.NotaInutilizada()
                {
                    Serie = nota.Serie,
                    Numero = nota.Numero,
                    Protocolo = nota.Protocolo,
                    DataInutilizacao = nota.DataInutilizacao,
                    Motivo = nota.Motivo
                }
            ).ToList();

            var NFeCanceladas = new List<NotaCancelada>();

            foreach (var nfe in nfeCanceladas)
            {
                var eventoCancelamento = eventoCancelamentoNFe.FirstOrDefault(e => e.NotaId == nfe.Id);

                var nfeCancelada = new NotaCancelada();
                nfeCancelada.DataCancelamento = eventoCancelamento.DataEvento.ToString();
                nfeCancelada.DataEmissao = nfe.DataEmissao.ToString();
                nfeCancelada.Numero = nfe.Numero;
                nfeCancelada.MotivoCancelamento = eventoCancelamento.MotivoCancelamento;
                nfeCancelada.ProtocoloCancelamento = eventoCancelamento.ProtocoloCancelamento;
                nfeCancelada.Serie = nfe.Serie;

                NFeCanceladas.Add(nfeCancelada);
            }

            List<NotaCancelada> NFCeCanceladas = new List<NotaCancelada>();

            foreach (var nfce in nfceCanceladas)
            {
                var eventoCancelamento = eventoCancelamentoNFCe.FirstOrDefault(e => e.NotaId == nfce.Id);

                var nfceCancelada = new NotaCancelada
                {
                    DataCancelamento = eventoCancelamento.DataEvento.ToString(),
                    DataEmissao = nfce.DataEmissao.ToString(),
                    Numero = nfce.Numero,
                    MotivoCancelamento = eventoCancelamento.MotivoCancelamento,
                    ProtocoloCancelamento = eventoCancelamento.ProtocoloCancelamento,
                    Serie = nfce.Serie
                };

                NFCeCanceladas.Add(nfceCancelada);
            }

            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            ReportViewer reportViewer = new ReportViewer();
            reportViewer.LocalReport.ReleaseSandboxAppDomain();

            ReportDataSource nfceAutorizadasDataSource = new ReportDataSource()
            {
                Name = "NFCe",
                Value = nfceAutorizadas
            };

            ReportDataSource nfeAutorizadasDataSource = new ReportDataSource()
            {
                Name = "NFe",
                Value = nfeAutorizadas
            };

            ReportDataSource nfceCanceladasDataSource = new ReportDataSource()
            {
                Name = "NFCeCanceladas",
                Value = NFCeCanceladas
            };

            ReportDataSource nfeCanceladasDataSource = new ReportDataSource()
            {
                Name = "NFeCanceladas",
                Value = NFeCanceladas
            };

            ReportDataSource emitenteDataSource = new ReportDataSource()
            {
                Name = "Emitente",
                Value = new List<Emitente>() { emitente }
            };

            ReportDataSource periodoDataSource = new ReportDataSource()
            {
                Name = "Periodo",
                Value = new List<DateTime>() { mesAno }
            };

            ReportDataSource nfeInutilizadasDataSource = new ReportDataSource()
            {
                Name = "NFeInutilizadas",
                Value = nfeInutilizadas
            };

            ReportDataSource nfceInutilizadasDataSource = new ReportDataSource()
            {
                Name = "NFCeInutilizadas",
                Value = nfceInutilizadas
            };

            reportViewer.LocalReport.DataSources.Add(nfceAutorizadasDataSource);
            reportViewer.LocalReport.DataSources.Add(nfeAutorizadasDataSource);
            reportViewer.LocalReport.DataSources.Add(emitenteDataSource);
            reportViewer.LocalReport.DataSources.Add(periodoDataSource);
            reportViewer.LocalReport.DataSources.Add(nfceCanceladasDataSource);
            reportViewer.LocalReport.DataSources.Add(nfeCanceladasDataSource);
            reportViewer.LocalReport.DataSources.Add(nfeInutilizadasDataSource);
            reportViewer.LocalReport.DataSources.Add(nfceInutilizadasDataSource);

            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), @"Reports\RelatorioGerencial.rdlc");

            byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

            string pathToPdf = Path.Combine(startPath, "RelatorioGerencial.pdf");
            File.WriteAllBytes(pathToPdf, bytes);
            reportViewer.Dispose();
        }

    }
}
