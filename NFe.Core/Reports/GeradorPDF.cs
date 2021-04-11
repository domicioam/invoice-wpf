using Microsoft.Reporting.WinForms;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Text;
using static QRCoder.PayloadGenerator;
using System.Drawing;

using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Domain;
using NFe.Core.Utils.PDF.RelatorioGerencial;

using System.Drawing.Imaging;
using Zen.Barcode;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using NFe.WPF.Model;
using System.Threading.Tasks;
using NFe.WPF.Model.ReportNFe;
using Pagamento = NFe.WPF.Model.Pagamento;
using Produto = NFe.WPF.Model.Produto;
using Transportadora = NFe.WPF.Model.ReportNFe.Transportadora;
using NFe.Core.NotasFiscais;
using System.Xml;

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
            var emitente = new Emitente()
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

            List<NotaInutilizada> nfceInutilizadas = notasInutilizadas.Where(n => n.Modelo.Equals(65)).Select(
                nota => new NotaInutilizada()
                {
                    Serie = nota.Serie,
                    Numero = nota.Numero,
                    Protocolo = nota.Protocolo,
                    DataInutilizacao = nota.DataInutilizacao,
                    Motivo = nota.Motivo
                }
                ).ToList();

            List<NotaInutilizada> nfeInutilizadas = notasInutilizadas.Where(n => n.Modelo.Equals(55)).Select(
                nota => new NotaInutilizada()
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

        static private System.Drawing.Point _PrintingDPI;
        static private int _PrintingIndex;
        static private List<Stream> _PrintingStreams = new List<Stream>();

        public static Task GerarPdfNotaFiscal(Domain.NotaFiscal notaFiscal)
        {
            return Task.Run(() =>
            {
                if (notaFiscal == null)
                {
                    throw new ArgumentNullException();
                }

                if (notaFiscal.Identificacao.Modelo == Modelo.Modelo55)
                {
                    GerarPDFNfe(notaFiscal);
                }
                else
                {
                    GerarPDFNfce(notaFiscal);
                }
            });
        }

        private static void GerarPDFNfce(Domain.NotaFiscal notaFiscal)
        {
            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            Byte[] data;
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

            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            using (ReportViewer reportViewer = new ReportViewer())
            {
                reportViewer.LocalReport.ReleaseSandboxAppDomain();

                var emitente = notaFiscal.Emitente;
                var destinatario = notaFiscal.Destinatario ?? new Domain.Destinatario("CONSUMIDOR NÃO IDENTIFICADO");

                var produtos = notaFiscal.Produtos.Select(produto => new Produto()
                {
                    Codigo = produto.Codigo,
                    Descricao = produto.Descricao,
                    Quantidade = produto.QtdeUnidadeComercial,
                    ValorUnitario = produto.ValorUnidadeComercial,
                    ValorTotal = produto.ValorTotal
                }).ToList();

                var pagamentos = notaFiscal.Pagamentos.ConvertAll(pagamento => new Pagamento()
                {
                    Nome = pagamento.FormaPagamentoTexto,
                    Valor = pagamento.Valor
                });

                var reportNFCeReadModel = new ReportNFCeReadModel
                {
                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
                    Numero = notaFiscal.Identificacao.Numero,
                    Serie = notaFiscal.Identificacao.Serie.ToString(),
                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                    Protocolo = notaFiscal.ProtocoloAutorizacao,
                    DataHoraAutorizacao = notaFiscal.DhAutorizacao.Replace("-", "/"),
                    InformacaoAdicional = notaFiscal.InfoAdicional.InfoAdicionalComplementar,
                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
                    QrCodeImage = data,
                    Emissor = new WPF.Model.Emissor()
                    {
                        CNPJ = emitente.CNPJ,
                        Nome = emitente.Nome,
                        Logradouro = emitente.Endereco.Logradouro,
                        Numero = emitente.Endereco.Numero,
                        Bairro = emitente.Endereco.Bairro,
                        Municipio = emitente.Endereco.Municipio,
                        UF = emitente.Endereco.UF,
                        CEP = emitente.Endereco.Cep
                    },
                    Destinatario = new WPF.Model.Destinatario()
                    {
                        Nome = destinatario.NomeRazao,
                        Documento = destinatario.Documento?.GetDocumentoDanfe(destinatario.TipoDestinatario),
                        Logradouro = destinatario.Endereco?.Logradouro,
                        Numero = destinatario.Endereco?.Numero,
                        Bairro = destinatario.Endereco?.Bairro,
                        Municipio = destinatario.Endereco?.Municipio,
                        UF = destinatario.Endereco?.UF,
                        CEP = destinatario.Endereco?.Cep
                    },
                    Produtos = produtos,
                    Pagamentos = pagamentos
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

                reportViewer.LocalReport.DataSources.Add(dadosDataSource);
                reportViewer.LocalReport.DataSources.Add(produtosDataSource);
                reportViewer.LocalReport.DataSources.Add(pagamentosDataSource);

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

        public static string ObterPdfEnvioNotaFiscalEmail(Domain.NotaFiscal notaFiscal)
        {
            Warning[] warnings;

            Byte[] data;
            Bitmap qrCodeAsBitmap;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                Url generator = new Url(notaFiscal.QrCodeUrl);
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

            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            using (var reportViewer = new ReportViewer())
            {
                reportViewer.LocalReport.ReleaseSandboxAppDomain();

                var emitente = notaFiscal.Emitente;
                var destinatario = notaFiscal.Destinatario ?? new Domain.Destinatario("CONSUMIDOR NÃO IDENTIFICADO");

                var produtos = notaFiscal.Produtos.Select(produto => new Produto()
                {
                    Codigo = produto.Codigo,
                    Descricao = produto.Descricao,
                    Quantidade = produto.QtdeUnidadeComercial,
                    ValorUnitario = produto.ValorUnidadeComercial,
                    ValorTotal = produto.ValorTotal
                }).ToList();

                var pagamentos = notaFiscal.Pagamentos.Select(pagamento => new Pagamento()
                {
                    Nome = pagamento.FormaPagamentoTexto,
                    Valor = pagamento.Valor
                }).ToList();

                var reportNFCeReadModel = new ReportNFCeReadModel
                {
                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
                    Numero = notaFiscal.Identificacao.Numero,
                    Serie = notaFiscal.Identificacao.Serie.ToString(),
                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                    Protocolo = notaFiscal.ProtocoloAutorizacao,
                    DataHoraAutorizacao = notaFiscal.DataHoraAutorização.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/"),
                    InformacaoAdicional = notaFiscal.InfoAdicional.InfoAdicionalComplementar,
                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
                    QrCodeImage = data,
                    Emissor = new WPF.Model.Emissor()
                    {
                        CNPJ = emitente.CNPJ,
                        Nome = emitente.Nome,
                        Logradouro = emitente.Endereco.Logradouro,
                        Numero = emitente.Endereco.Numero,
                        Bairro = emitente.Endereco.Bairro,
                        Municipio = emitente.Endereco.Municipio,
                        UF = emitente.Endereco.UF,
                        CEP = emitente.Endereco.Cep
                    },
                    Destinatario = new WPF.Model.Destinatario()
                    {
                        Nome = destinatario.NomeRazao,
                        Documento = destinatario.Documento?.GetDocumentoDanfe(destinatario.TipoDestinatario),
                        Logradouro = destinatario.Endereco?.Logradouro,
                        Numero = destinatario.Endereco?.Numero,
                        Bairro = destinatario.Endereco?.Bairro,
                        Municipio = destinatario.Endereco?.Municipio,
                        UF = destinatario.Endereco?.UF,
                        CEP = destinatario.Endereco?.Cep
                    },
                    Produtos = produtos,
                    Pagamentos = pagamentos
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

                reportViewer.LocalReport.DataSources.Add(dadosDataSource);
                reportViewer.LocalReport.DataSources.Add(produtosDataSource);
                reportViewer.LocalReport.DataSources.Add(pagamentosDataSource);

                reportViewer.ProcessingMode = ProcessingMode.Local;
                reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), @"Reports\ReportNfceEmail.rdlc");

                byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out _, out _, out _, out _, out warnings);

                string pathToPdf = Path.Combine(Path.GetTempPath(), notaFiscal.Identificacao.Chave + ".pdf");
                File.WriteAllBytes(pathToPdf, bytes);

                return pathToPdf;
            }

        }

        private static void GerarPDFNfe(Domain.NotaFiscal notaFiscal)
        {
            if (notaFiscal.Identificacao.Ambiente == Ambiente.Homologacao)
            {
                notaFiscal.InfoAdicional.InfoAdicionalComplementar = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
            }

            var destinatario = notaFiscal.Destinatario;

            var barcode = BarcodeDrawFactory.Code128WithChecksum;
            var barcodeImg = barcode.Draw(notaFiscal.Identificacao.Chave.ToString(), 36);

            // QrCode
            Byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                barcodeImg.Save(memoryStream, ImageFormat.Png);
                data = memoryStream.ToArray();
            }

            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            var produtos = notaFiscal.Produtos.ConvertAll(produto => new WPF.Model.ReportNFe.Produto()
            {
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                Ncm = produto.Ncm,
                Cst = produto.CstTexto,
                Cfop = produto.CfopTexto,
                UnidadeComercial = produto.UnidadeComercial,
                Quantidade = produto.QtdeUnidadeComercial,
                ValorUnitario = produto.ValorUnidadeComercial,
                ValorDesconto = produto.Desconto,
                ValorLiquido = produto.ValorTotal
            });

            using (var reportViewer = new ReportViewer())
            {
                reportViewer.LocalReport.ReleaseSandboxAppDomain();
                var emitente = notaFiscal.Emitente;
                var calculoImposto = notaFiscal.TotalNFe.IcmsTotal;
                var transportadora = notaFiscal.Transporte.Transportadora;
                var veiculo = notaFiscal.Transporte.Veiculo;

                var reportNfeReadModel = new ReportNFeReadModel()
                {
                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
                    Numero = notaFiscal.Identificacao.Numero,
                    Serie = notaFiscal.Identificacao.Serie.ToString(),
                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                    ProtocoloAutorizacao = notaFiscal.ProtocoloAutorizacao,
                    DataHoraAutorizacao = notaFiscal.DhAutorizacao.Replace("-", "/"),
                    DataSaida = notaFiscal.Identificacao.DataSaida,
                    HoraSaida = notaFiscal.Identificacao.HoraSaida,
                    InformacaoAdicional = notaFiscal.InfoAdicional.InfoAdicionalComplementar,
                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
                    NaturezaOperacao = notaFiscal.Identificacao.NaturezaOperacao,
                    TipoOperacao = notaFiscal.Identificacao.TipoOperacaooTexto,
                    BarcodeImage = data,
                    Emissor = new WPF.Model.ReportNFe.Emissor()
                    {
                        CNPJ = emitente.CNPJ,
                        InscricaoEstadual = emitente.InscricaoEstadual,
                        NomeRazao = emitente.Nome,
                        NomeFantasia = emitente.NomeFantasia,
                        Logradouro = emitente.Endereco.Logradouro,
                        Numero = emitente.Endereco.Numero,
                        Bairro = emitente.Endereco.Bairro,
                        Municipio = emitente.Endereco.Municipio,
                        UF = emitente.Endereco.UF,
                        CEP = emitente.Endereco.Cep,
                        Telefone = emitente.Telefone
                    },
                    Destinatario = new WPF.Model.ReportNFe.Destinatario()
                    {
                        Nome = destinatario.NomeRazao,
                        Documento = destinatario.Documento.Numero,
                        InscricaoEstadual = destinatario.InscricaoEstadual,
                        Logradouro = destinatario.Endereco?.Logradouro,
                        Numero = destinatario.Endereco?.Numero,
                        Bairro = destinatario.Endereco?.Bairro,
                        Municipio = destinatario.Endereco?.Municipio,
                        UF = destinatario.Endereco?.UF,
                        CEP = destinatario.Endereco?.Cep,
                        Telefone = destinatario.Telefone
                    },
                    Produtos = produtos,
                    CalculoImposto = new CalculoImposto()
                    {
                        BaseCalculo = calculoImposto.BaseCalculo,
                        BaseCalculoST = calculoImposto.BaseCalculoST,
                        ValorDespesasAcessorias = calculoImposto.TotalOutros,
                        ValorTotalAproximado = calculoImposto.ValorTotalAproximadoTributos,
                        ValorTotalCofins = calculoImposto.ValorTotalCofins,
                        ValorTotalDesconto = calculoImposto.ValorTotalDesconto,
                        ValorTotalDesonerado = calculoImposto.ValorTotalDesonerado,
                        ValorTotalFrete = calculoImposto.ValorTotalFrete,
                        ValorTotalII = calculoImposto.ValorTotalII,
                        ValorTotalIcms = calculoImposto.ValorTotalIcms,
                        ValorTotalIpi = calculoImposto.ValorTotalIpi,
                        ValorTotalNFe = calculoImposto.ValorTotalNFe,
                        ValorTotalPis = calculoImposto.ValorTotalPis,
                        ValorTotalProdutos = calculoImposto.ValorTotalProdutos,
                        ValorTotalST = calculoImposto.ValorTotalST,
                        ValorTotalSeguro = calculoImposto.ValorTotalSeguro
                    },
                    Transportadora = new Transportadora()
                    {
                        InscricaoEstadual = transportadora.InscricaoEstadual,
                        CodigoANT = veiculo.RegistroAntt,
                        CpfCnpj = transportadora.CpfCnpj,
                        EnderecoCompleto = transportadora.EnderecoCompleto,
                        Municipio = transportadora.Municipio,
                        Nome = transportadora.Nome,
                        UF = transportadora.SiglaUF,
                        VeiculoPlaca = veiculo.Placa,
                        VeiculoUF = veiculo.SiglaUF,
                        ModalidadeFrete = "9 - Sem Frete"
                    }
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
            ev.HasMorePages = (_PrintingIndex < _PrintingStreams.Count);
        }

        private static Stream CreateStream(string name,
          string fileNameExtension, Encoding encoding,
          string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            _PrintingStreams.Add(stream);
            return stream;
        }

    }
}
