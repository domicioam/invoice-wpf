using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using Zen.Barcode;

namespace DgSystem.NFe.Reports.Nfe
{
    public class DanfeNfe
    {
        public static void GerarPDFNfe(NotaFiscal notaFiscal)
        {
            var barcode = BarcodeDrawFactory.Code128WithChecksum;
            var barcodeImg = barcode.Draw(notaFiscal.Identificacao.Chave.ToString(), 36);

            // QrCode
            Byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                barcodeImg.Save(memoryStream, ImageFormat.Png);
                data = memoryStream.ToArray();
            }

            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            using (var reportViewer = new ReportViewer())
            {
                reportViewer.LocalReport.ReleaseSandboxAppDomain();
                var emitente = notaFiscal.Emitente;
                var calculoImposto = notaFiscal.IcmsTotal;
                var transportadora = notaFiscal.Transportadora;
                var veiculo = notaFiscal.Veiculo;

                var reportNfeReadModel = new ReportNFeReadModel()
                {
                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
                    Numero = notaFiscal.Identificacao.Numero,
                    Serie = notaFiscal.Identificacao.Serie,
                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                    ProtocoloAutorizacao = notaFiscal.ProtocoloAutorizacao,
                    DataHoraAutorizacao = notaFiscal.DhAutorizacao.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/"),
                    DataSaida = notaFiscal.Identificacao.DataSaida,
                    HoraSaida = notaFiscal.Identificacao.HoraSaida,
                    InformacaoAdicional = notaFiscal.InfoAdicionalComplementar,
                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
                    NaturezaOperacao = notaFiscal.Identificacao.NaturezaOperacao,
                    TipoOperacao = notaFiscal.Identificacao.TipoOperacaooTexto,
                    BarcodeImage = data,
                    Emissor = notaFiscal.Emitente,
                    Destinatario = notaFiscal.Destinatario,
                    Produtos = notaFiscal.Produtos,
                    CalculoImposto = notaFiscal.IcmsTotal,
                    Transportadora = notaFiscal.Transportadora
                };

                var dadosDataSource = new ReportDataSource()
                {
                    Name = "Dados",
                    Value = new List<ReportNFeReadModel>() { reportNfeReadModel }
                };

                var produtosDataSource = new ReportDataSource()
                {
                    Name = "Produtos",
                    Value = reportNfeReadModel.Produtos
                };

                reportViewer.LocalReport.DataSources.Add(dadosDataSource);
                reportViewer.LocalReport.DataSources.Add(produtosDataSource);

                reportViewer.ProcessingMode = ProcessingMode.Local;
                reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), @"Reports\ReportNfe.rdlc");

                byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out _, out _, out _, out _, out _);

                string pathToPdf = Path.Combine(Path.GetTempPath(), "temppdf_" + notaFiscal.Identificacao.Chave + ".pdf");
                File.WriteAllBytes(pathToPdf, bytes);
                Process.Start(pathToPdf);
            }
        }
    }
}
