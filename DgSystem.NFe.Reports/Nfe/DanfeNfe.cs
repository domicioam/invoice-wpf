//using Microsoft.Reporting.WinForms;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DgSystem.NFe.Reports
//{
//    public class DanfeNfe
//    {
//        public static void GerarPDFNfe(NotaFiscal notaFiscal)
//        {
//            if (notaFiscal.Identificacao.Ambiente == Ambiente.Homologacao)
//            {
//                notaFiscal.InfoAdicionalComplementar = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
//            }

//            var destinatario = notaFiscal.Destinatario;

//            var barcode = BarcodeDrawFactory.Code128WithChecksum;
//            var barcodeImg = barcode.Draw(notaFiscal.Identificacao.Chave.ToString(), 36);

//            // QrCode
//            Byte[] data;

//            using (var memoryStream = new MemoryStream())
//            {
//                barcodeImg.Save(memoryStream, ImageFormat.Png);
//                data = memoryStream.ToArray();
//            }

//            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

//            var produtos = notaFiscal.Produtos.ConvertAll(produto => new WPF.Model.ReportNFe.Produto()
//            {
//                Codigo = produto.Codigo,
//                Descricao = produto.Descricao,
//                Ncm = produto.Ncm,
//                Cst = produto.CstTexto,
//                Cfop = produto.CfopTexto,
//                UnidadeComercial = produto.UnidadeComercial,
//                Quantidade = produto.QtdeUnidadeComercial,
//                ValorUnitario = produto.ValorUnidadeComercial,
//                ValorDesconto = produto.Desconto,
//                ValorLiquido = produto.ValorTotal
//            });

//            using (var reportViewer = new ReportViewer())
//            {
//                reportViewer.LocalReport.ReleaseSandboxAppDomain();
//                var emitente = notaFiscal.Emitente;
//                var calculoImposto = notaFiscal.TotalNFe.IcmsTotal;
//                var transportadora = notaFiscal.Transporte.Transportadora;
//                var veiculo = notaFiscal.Transporte.Veiculo;

//                var reportNfeReadModel = new ReportNFeReadModel()
//                {
//                    Chave = notaFiscal.Identificacao.Chave.ChaveMasked,
//                    Numero = notaFiscal.Identificacao.Numero,
//                    Serie = notaFiscal.Identificacao.Serie.ToString(),
//                    DataHoraEmissao = notaFiscal.Identificacao.DataHoraEmissao,
//                    Protocolo = notaFiscal.ProtocoloAutorizacao,
//                    DataHoraAutorizacao = notaFiscal.DhAutorizacao.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/"),
//                    DataSaida = notaFiscal.Identificacao.DataSaida,
//                    HoraSaida = notaFiscal.Identificacao.HoraSaida,
//                    InformacaoAdicional = notaFiscal.InfoAdicionalComplementar,
//                    LinkConsultaChave = notaFiscal.Identificacao.LinkConsultaChave,
//                    InformacaoInteresse = notaFiscal.Identificacao.MensagemInteresseContribuinte,
//                    QuantidadeTotalProdutos = notaFiscal.QtdTotalProdutos,
//                    ValorTotalProdutos = notaFiscal.ValorTotalProdutos,
//                    NaturezaOperacao = notaFiscal.Identificacao.NaturezaOperacao,
//                    TipoOperacao = notaFiscal.Identificacao.TipoOperacaooTexto,
//                    BarcodeImage = data,
//                    Emissor = notaFiscal.Emitente,
//                    Destinatario = notaFiscal.Destinatario,
//                    Produtos = produtos,
//                    CalculoImposto = new CalculoImposto()
//                    {
//                        BaseCalculo = calculoImposto.BaseCalculo,
//                        BaseCalculoST = calculoImposto.BaseCalculoST,
//                        ValorDespesasAcessorias = calculoImposto.TotalOutros,
//                        ValorTotalAproximado = calculoImposto.ValorTotalAproximadoTributos,
//                        ValorTotalCofins = calculoImposto.ValorTotalCofins,
//                        ValorTotalDesconto = calculoImposto.ValorTotalDesconto,
//                        ValorTotalDesonerado = calculoImposto.ValorTotalDesonerado,
//                        ValorTotalFrete = calculoImposto.ValorTotalFrete,
//                        ValorTotalII = calculoImposto.ValorTotalII,
//                        ValorTotalIcms = calculoImposto.ValorTotalIcms,
//                        ValorTotalIpi = calculoImposto.ValorTotalIpi,
//                        ValorTotalNFe = calculoImposto.ValorTotalNFe,
//                        ValorTotalPis = calculoImposto.ValorTotalPis,
//                        ValorTotalProdutos = calculoImposto.ValorTotalProdutos,
//                        ValorTotalST = calculoImposto.ValorTotalST,
//                        ValorTotalSeguro = calculoImposto.ValorTotalSeguro
//                    },
//                    Transportadora = new Transportadora()
//                    {
//                        InscricaoEstadual = transportadora.InscricaoEstadual,
//                        CodigoANT = veiculo.RegistroAntt,
//                        CpfCnpj = transportadora.CpfCnpj,
//                        EnderecoCompleto = transportadora.EnderecoCompleto,
//                        Municipio = transportadora.Municipio,
//                        Nome = transportadora.Nome,
//                        UF = transportadora.SiglaUF,
//                        VeiculoPlaca = veiculo.Placa,
//                        VeiculoUF = veiculo.SiglaUF,
//                        ModalidadeFrete = "9 - Sem Frete"
//                    }
//                };

//                var dadosDataSource = new ReportDataSource()
//                {
//                    Name = "Dados",
//                    Value = new List<ReportNFeReadModel>() { reportNfeReadModel }
//                };

//                var produtosDataSource = new ReportDataSource()
//                {
//                    Name = "Produtos",
//                    Value = reportNfeReadModel.Produtos
//                };

//                reportViewer.LocalReport.DataSources.Add(dadosDataSource);
//                reportViewer.LocalReport.DataSources.Add(produtosDataSource);

//                reportViewer.ProcessingMode = ProcessingMode.Local;
//                reportViewer.LocalReport.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), @"Reports\ReportNfe.rdlc");

//                byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out _, out _, out _, out _, out _);

//                string pathToPdf = Path.Combine(Path.GetTempPath(), "temppdf_" + notaFiscal.Identificacao.Chave + ".pdf");
//                File.WriteAllBytes(pathToPdf, bytes);
//                Process.Start(pathToPdf);
//            }
//        }
//    }
//}
