using Microsoft.Reporting.WinForms;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using static QRCoder.PayloadGenerator;

namespace DgSystem.NFe.Reports.Nfce
{
    public class DanfeNfce
    {
        static private Point _PrintingDPI;
        static private int _PrintingIndex;
        static private List<Stream> _PrintingStreams = new List<Stream>();

        public static void GerarPDFNfce(NotaFiscal notaFiscal)
        {
            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            byte[] data;
            Bitmap qrCodeAsBitmap;
            string qrCodeUrl = GetQrCodeUrl(notaFiscal.QrCodeUrl);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                Url generator = new Url(qrCodeUrl);
                string payload = generator.ToString();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                qrCodeAsBitmap = qrCode.GetGraphic(10);
            }

            using (var memoryStream = new MemoryStream())
            {
                qrCodeAsBitmap.Save(memoryStream, ImageFormat.Bmp);
                data = memoryStream.ToArray();
            }

            notaFiscal.Identificacao.QrCodeImage = data;

            // SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            using (ReportViewer reportViewer = new ReportViewer())
            {
                reportViewer.LocalReport.ReleaseSandboxAppDomain();

                var emitente = notaFiscal.Emitente;
                var destinatario = notaFiscal.Destinatario ?? new Destinatario("CONSUMIDOR NÃO IDENTIFICADO");

                var totaisNotaFiscal = new List<ItemTotal> {
                    new ItemTotal("Valor total R$", notaFiscal.ValorTotalProdutos)
                };

                double totalDesconto = notaFiscal.Produtos.Sum(p => p.Desconto);
                if (totalDesconto > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Desconto R$", totalDesconto));
                }

                double totalFrete = notaFiscal.Produtos.Sum(p => p.Frete);
                if (totalFrete > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Frete R$", totalFrete));
                }

                double totalSeguro = notaFiscal.Produtos.Sum(p => p.Seguro);
                if (totalSeguro > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Seguro R$", totalSeguro));
                }

                double totalOutros = notaFiscal.Produtos.Sum(p => p.Outros);
                if (totalOutros > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Outros R$", totalOutros));
                }

                totaisNotaFiscal.Add(new ItemTotal("Valor a Pagar R$", notaFiscal.ValorTotalProdutos - totalDesconto + totalFrete + totalSeguro + totalOutros));

                var reportNFCeReadModel = new ReportNFCeReadModel
                {
                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
                    Numero = notaFiscal.Identificacao.Numero,
                    Serie = notaFiscal.Identificacao.Serie,
                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                    Protocolo = notaFiscal.ProtocoloAutorizacao,
                    DataHoraAutorizacao = notaFiscal.DhAutorizacao.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/"),
                    InformacaoAdicional = notaFiscal.InfoAdicionalComplementar,
                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
                    QrCodeImage = data,
                    Emissor = notaFiscal.Emitente,
                    Destinatario = notaFiscal.Destinatario,
                    Produtos = notaFiscal.Produtos.ToList(),
                    Pagamentos = notaFiscal.Pagamentos.ToList(),
                    TotaisNotaFiscal = totaisNotaFiscal
                };

                ReportDataSource dadosDataSource = new ReportDataSource()
                {
                    Name = "Dados",
                    Value = new List<ReportNFCeReadModel>() { reportNFCeReadModel }
                };

                ReportDataSource produtosDataSource = new ReportDataSource()
                {
                    Name = "Produtos",
                    Value = reportNFCeReadModel.Produtos
                };

                ReportDataSource pagamentosDataSource = new ReportDataSource()
                {
                    Name = "Pagamentos",
                    Value = reportNFCeReadModel.Pagamentos
                };

                ReportDataSource totaisNotaFiscalDataSource = new ReportDataSource()
                {
                    Name = "TotaisNotaFiscal",
                    Value = reportNFCeReadModel.TotaisNotaFiscal
                };

                reportViewer.LocalReport.DataSources.Add(dadosDataSource);
                reportViewer.LocalReport.DataSources.Add(produtosDataSource);
                reportViewer.LocalReport.DataSources.Add(pagamentosDataSource);
                reportViewer.LocalReport.DataSources.Add(totaisNotaFiscalDataSource);

                reportViewer.ProcessingMode = ProcessingMode.Local;
                reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), @"Reports\ReportNfce.rdlc");
#if DEBUG

                byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                string pathToPdf = Path.Combine(Path.GetTempPath(), "temppdf_" + notaFiscal.Identificacao.Chave + ".pdf");
                File.WriteAllBytes(pathToPdf, bytes);
                Process.Start(pathToPdf);
#else
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = @"EPSON TM-T20 Receipt";
                printDoc.PrintPage += PrintPage;
                printDoc.EndPrint += EndPrint;

                _PrintingDPI.X = printDoc.DefaultPageSettings.PrinterResolution.X;
                _PrintingDPI.Y = printDoc.DefaultPageSettings.PrinterResolution.Y;

                var sb = new StringBuilder();
                var xr = XmlWriter.Create(sb);
                xr.WriteStartElement("DeviceInfo");
                xr.WriteElementString("OutputFormat", "EMF");
                xr.WriteElementString("PrintDpiX", _PrintingDPI.X.ToString());
                xr.WriteElementString("PrintDpiY", _PrintingDPI.Y.ToString());
                xr.WriteElementString("PageWidth", "3.14961in");
                xr.WriteElementString("MarginLeft", "0.118in");
                xr.Close();

                _PrintingStreams = new List<Stream>();

                reportViewer.LocalReport.Render("Image", sb.ToString(), CreateStream, out warnings);

                foreach (var s in _PrintingStreams)
                    s.Position = 0;

                printDoc.Print();
#endif
            }
        }

        public static string ObterPdfEnvioNotaFiscalEmail(NotaFiscal notaFiscal)
        {
            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            byte[] data;
            Bitmap qrCodeAsBitmap;
            string qrCodeUrl = GetQrCodeUrl(notaFiscal.QrCodeUrl);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                Url generator = new Url(qrCodeUrl);
                string payload = generator.ToString();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                qrCodeAsBitmap = qrCode.GetGraphic(10);
            }

            using (var memoryStream = new MemoryStream())
            {
                qrCodeAsBitmap.Save(memoryStream, ImageFormat.Bmp);
                data = memoryStream.ToArray();
            }

            notaFiscal.Identificacao.QrCodeImage = data;

            // SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            using (ReportViewer reportViewer = new ReportViewer())
            {
                reportViewer.LocalReport.ReleaseSandboxAppDomain();

                var emitente = notaFiscal.Emitente;
                var destinatario = notaFiscal.Destinatario ?? new Destinatario("CONSUMIDOR NÃO IDENTIFICADO");

                var totaisNotaFiscal = new List<ItemTotal> {
                    new ItemTotal("Valor total R$", notaFiscal.ValorTotalProdutos)
                };

                double totalDesconto = notaFiscal.Produtos.Sum(p => p.Desconto);
                if (totalDesconto > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Desconto R$", totalDesconto));
                }

                double totalFrete = notaFiscal.Produtos.Sum(p => p.Frete);
                if (totalFrete > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Frete R$", totalFrete));
                }

                double totalSeguro = notaFiscal.Produtos.Sum(p => p.Seguro);
                if (totalSeguro > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Seguro R$", totalSeguro));
                }

                double totalOutros = notaFiscal.Produtos.Sum(p => p.Outros);
                if (totalOutros > 0)
                {
                    totaisNotaFiscal.Add(new ItemTotal("Outros R$", totalOutros));
                }

                totaisNotaFiscal.Add(new ItemTotal("Valor a Pagar R$", notaFiscal.ValorTotalProdutos - totalDesconto + totalFrete + totalSeguro + totalOutros));

                var reportNFCeReadModel = new ReportNFCeReadModel
                {
                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
                    Numero = notaFiscal.Identificacao.Numero,
                    Serie = notaFiscal.Identificacao.Serie,
                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                    Protocolo = notaFiscal.ProtocoloAutorizacao,
                    DataHoraAutorizacao = notaFiscal.DhAutorizacao.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/"),
                    InformacaoAdicional = notaFiscal.InfoAdicionalComplementar,
                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
                    QrCodeImage = data,
                    Emissor = notaFiscal.Emitente,
                    Destinatario = notaFiscal.Destinatario,
                    Produtos = notaFiscal.Produtos.ToList(),
                    Pagamentos = notaFiscal.Pagamentos.ToList(),
                    TotaisNotaFiscal = totaisNotaFiscal
                };

                ReportDataSource dadosDataSource = new ReportDataSource()
                {
                    Name = "Dados",
                    Value = new List<ReportNFCeReadModel>() { reportNFCeReadModel }
                };

                ReportDataSource produtosDataSource = new ReportDataSource()
                {
                    Name = "Produtos",
                    Value = reportNFCeReadModel.Produtos
                };

                ReportDataSource pagamentosDataSource = new ReportDataSource()
                {
                    Name = "Pagamentos",
                    Value = reportNFCeReadModel.Pagamentos
                };

                ReportDataSource totaisNotaFiscalDataSource = new ReportDataSource()
                {
                    Name = "TotaisNotaFiscal",
                    Value = reportNFCeReadModel.TotaisNotaFiscal
                };

                reportViewer.LocalReport.DataSources.Add(dadosDataSource);
                reportViewer.LocalReport.DataSources.Add(produtosDataSource);
                reportViewer.LocalReport.DataSources.Add(pagamentosDataSource);
                reportViewer.LocalReport.DataSources.Add(totaisNotaFiscalDataSource);

                reportViewer.ProcessingMode = ProcessingMode.Local;
                reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), @"Reports\ReportNfceEmail.rdlc");

                byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out _, out _, out _, out _, out warnings);

                string pathToPdf = Path.Combine(Path.GetTempPath(), notaFiscal.Identificacao.Chave + ".pdf");
                File.WriteAllBytes(pathToPdf, bytes);

                return pathToPdf;
            }
        }

        private static string GetQrCodeUrl(string text, string firstString = "<![CDATA[", string lastString = "]]>")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException();
            }

            if (!text.Contains("<![CDATA["))
            {
                firstString = "<qrCode>";
                lastString = "</qrCode>";
            }

            if (!text.Contains(firstString))
            {
                return text;
            }

            int pos1 = text.IndexOf(firstString) + firstString.Length;
            int pos2 = text.IndexOf(lastString);
            return text.Substring(pos1, pos2 - pos1);
        }

        private static void EndPrint(object sender, PrintEventArgs e)
        {
            foreach (var s in _PrintingStreams)
                s.Dispose();
            _PrintingStreams.Clear();
            _PrintingIndex = 0;
        }

        private static void PrintPage(object sender, PrintPageEventArgs ev)
        {
            Rectangle adjustedRect = new Rectangle(
                ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                ev.PageBounds.Width,
                ev.PageBounds.Height);

            var mf = new Metafile(_PrintingStreams[_PrintingIndex]);
            var mfh = mf.GetMetafileHeader();
            ev.Graphics.ScaleTransform(mfh.DpiX / _PrintingDPI.X, mfh.DpiY /
                                      _PrintingDPI.Y, MatrixOrder.Prepend);

            ev.Graphics.DrawImageUnscaled(mf, adjustedRect);

            _PrintingIndex++;
            ev.HasMorePages = _PrintingIndex < _PrintingStreams.Count;
        }

        private static Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            _PrintingStreams.Add(stream);
            return stream;
        }
    }
}
