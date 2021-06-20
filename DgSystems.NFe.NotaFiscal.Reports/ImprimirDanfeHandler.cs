using DgSystem.NFe.Reports.Nfce;
using MediatR;
using NFe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DgSystems.NFe.NotaFiscal.Reports
{
    public class ImprimirDanfeHandler : IRequestHandler<ImprimirDanfe, bool>, IRequestHandler<global::NFe.Core.Domain.GerarDanfeNfceEmail, string>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // send message from NotaFiscal bounded context to Reports bounded context
        public Task<bool> Handle(ImprimirDanfe request, CancellationToken cancellationToken)
        {
            // move qrcode generation to DgSystems.NFe.Reports

            log.Info("Mensagem para imprimir danfe recebida.");

            try
            {
                if (request.NotaFiscal.Identificacao.Modelo == global::NFe.Core.Domain.Modelo.Modelo65)
                {
                    Identificacao identificacao
                        = new Identificacao(
                            new Chave(request.NotaFiscal.Identificacao.Chave.ChaveMasked),
                            request.NotaFiscal.Identificacao.Numero,
                            request.NotaFiscal.Identificacao.Serie.ToString(),
                            request.NotaFiscal.Identificacao.DataHoraEmissao,
                            request.NotaFiscal.Identificacao.LinkConsultaChave,
                            request.NotaFiscal.Identificacao.MensagemInteresseContribuinte);

                    Emitente emitente
                        = new Emitente(
                            request.NotaFiscal.Emitente.CNPJ,
                            request.NotaFiscal.Emitente.Endereco.Logradouro,
                            request.NotaFiscal.Emitente.Nome,
                            request.NotaFiscal.Emitente.Endereco.Numero,
                            request.NotaFiscal.Emitente.Endereco.Bairro,
                            request.NotaFiscal.Emitente.Endereco.Municipio,
                            request.NotaFiscal.Emitente.Endereco.UF,
                            request.NotaFiscal.Emitente.Endereco.Cep);

                    Destinatario destinatario
                        = new Destinatario(
                            request.NotaFiscal.Destinatario?.NomeRazao,
                            request.NotaFiscal.Destinatario?.Documento.Numero,
                            request.NotaFiscal.Destinatario?.Endereco?.Logradouro,
                            request.NotaFiscal.Destinatario?.Endereco?.Numero,
                            request.NotaFiscal.Destinatario?.Endereco?.Bairro,
                            request.NotaFiscal.Destinatario?.Endereco?.Municipio,
                            request.NotaFiscal.Destinatario?.Endereco?.UF,
                            request.NotaFiscal.Destinatario?.Endereco?.Cep);

                    IEnumerable<Produto> produtos =
                        request.NotaFiscal.Produtos
                        .Select(p =>
                            new Produto(
                                p.Codigo,
                                p.Descricao,
                                p.ValorUnidadeComercial,
                                p.ValorTotal,
                                p.QtdeUnidadeComercial,
                                p.Desconto,
                                p.Frete,
                                p.Seguro,
                                p.Outros));

                    IEnumerable<Pagamento> pagamentos =
                        request.NotaFiscal.Pagamentos
                        .Select(p =>
                            new Pagamento(p.FormaPagamentoTexto, p.Valor));

                    var notaFiscal
                        = new DgSystem.NFe.Reports.Nfce.NotaFiscal(
                            request.NotaFiscal.QrCodeUrl,
                            identificacao,
                            emitente,
                            destinatario,
                            produtos,
                            pagamentos,
                            request.NotaFiscal.ValorTotalProdutos,
                            request.NotaFiscal.ProtocoloAutorizacao,
                            request.NotaFiscal.DataHoraAutorização,
                            request.NotaFiscal.InfoAdicional.InfoAdicionalComplementar,
                            request.NotaFiscal.QtdTotalProdutos);

                    DanfeNfce.GerarPDFNfce(notaFiscal);
                }
                else
                {
                    string infoAdicionalComplementar;
                    if (request.NotaFiscal.Identificacao.Ambiente == global::NFe.Core.Domain.Ambiente.Homologacao)
                    {
                        infoAdicionalComplementar = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
                    }
                    else
                    {
                        infoAdicionalComplementar = request.NotaFiscal.InfoAdicional.InfoAdicionalComplementar;
                    }

                    DgSystem.NFe.Reports.Nfe.Identificacao identificacao
                        = new DgSystem.NFe.Reports.Nfe.Identificacao(
                            new DgSystem.NFe.Reports.Nfe.Chave(request.NotaFiscal.Identificacao.Chave.ChaveMasked),
                            request.NotaFiscal.Identificacao.Numero,
                            request.NotaFiscal.Identificacao.Serie.ToString(),
                            request.NotaFiscal.Identificacao.DataHoraEmissao,
                            request.NotaFiscal.Identificacao.LinkConsultaChave,
                            request.NotaFiscal.Identificacao.MensagemInteresseContribuinte,
                            request.NotaFiscal.Identificacao.DataSaida,
                            request.NotaFiscal.Identificacao.HoraSaida,
                            request.NotaFiscal.Identificacao.NaturezaOperacao,
                            request.NotaFiscal.Identificacao.TipoOperacaooTexto);

                    DgSystem.NFe.Reports.Nfe.Emitente emitente
                        = new DgSystem.NFe.Reports.Nfe.Emitente(
                            request.NotaFiscal.Emitente.CNPJ,
                            request.NotaFiscal.Emitente.InscricaoEstadual,
                            request.NotaFiscal.Emitente.Nome,
                            request.NotaFiscal.Emitente.NomeFantasia,
                            request.NotaFiscal.Emitente.Endereco.Logradouro,
                            request.NotaFiscal.Emitente.Endereco.Numero,
                            request.NotaFiscal.Emitente.Endereco.Bairro,
                            request.NotaFiscal.Emitente.Endereco.Municipio,
                            request.NotaFiscal.Emitente.Endereco.UF,
                            request.NotaFiscal.Emitente.Endereco.Cep,
                            request.NotaFiscal.Emitente.Telefone);

                    DgSystem.NFe.Reports.Nfe.Destinatario destinatario
                        = new DgSystem.NFe.Reports.Nfe.Destinatario(
                            request.NotaFiscal.Destinatario.NomeRazao,
                            request.NotaFiscal.Destinatario.Documento.Numero,
                            request.NotaFiscal.Destinatario.InscricaoEstadual,
                            request.NotaFiscal.Destinatario.Endereco.Logradouro,
                            request.NotaFiscal.Destinatario.Endereco.Numero,
                            request.NotaFiscal.Destinatario.Endereco.Bairro,
                            request.NotaFiscal.Destinatario.Endereco.Municipio,
                            request.NotaFiscal.Destinatario.Endereco.UF,
                            request.NotaFiscal.Destinatario.Endereco.Cep,
                            request.NotaFiscal.Destinatario.Telefone);

                    IEnumerable<DgSystem.NFe.Reports.Nfe.Produto> produtos =
                        request.NotaFiscal.Produtos
                        .Select(p =>
                            new DgSystem.NFe.Reports.Nfe.Produto(
                                p.Codigo,
                                p.Descricao,
                                p.Ncm,
                                p.CstTexto,
                                p.CfopTexto,
                                p.UnidadeComercial,
                                p.QtdeUnidadeComercial,
                                p.ValorUnidadeComercial,
                                p.Desconto,
                                p.ValorTotal));

                    IEnumerable<DgSystem.NFe.Reports.Nfe.Pagamento> pagamentos =
                        request.NotaFiscal.Pagamentos
                        .Select(p =>
                            new DgSystem.NFe.Reports.Nfe.Pagamento(p.FormaPagamentoTexto, p.Valor));

                    var veiculo =
                        new DgSystem.NFe.Reports.Nfe.Veiculo(
                            request.NotaFiscal.Transporte.Veiculo.Placa,
                            request.NotaFiscal.Transporte.Veiculo.SiglaUF,
                            request.NotaFiscal.Transporte.Veiculo.RegistroAntt);

                    var transportadora =
                        new DgSystem.NFe.Reports.Nfe.Transportadora(
                            request.NotaFiscal.Transporte.Transportadora.Nome,
                            "9 - Sem Frete",
                            veiculo.RegistroAntt,
                            request.NotaFiscal.Transporte.Transportadora.CpfCnpj,
                            veiculo.SiglaUF,
                            request.NotaFiscal.Transporte.Transportadora.CpfCnpj,
                            request.NotaFiscal.Transporte.Transportadora.EnderecoCompleto,
                            request.NotaFiscal.Transporte.Transportadora.Municipio,
                            request.NotaFiscal.Transporte.Transportadora.SiglaUF,
                            request.NotaFiscal.Transporte.Transportadora.InscricaoEstadual,
                            request.NotaFiscal.Transporte.Transportadora.SiglaUF);

                    var calculoImposto =
                        new DgSystem.NFe.Reports.Nfe.CalculoImposto(
                            request.NotaFiscal.TotalNFe.IcmsTotal.BaseCalculo,
                            request.NotaFiscal.TotalNFe.IcmsTotal.BaseCalculoST,
                            request.NotaFiscal.TotalNFe.IcmsTotal.TotalOutros,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalAproximadoTributos,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalCofins,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalDesconto,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalDesonerado,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalFrete,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalII,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalIpi,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalPis,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalProdutos,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalSeguro,
                            request.NotaFiscal.TotalNFe.IcmsTotal.ValorTotalST);

                    var notaFiscal
                        = new DgSystem.NFe.Reports.Nfe.NotaFiscal(
                            request.NotaFiscal.QrCodeUrl,
                            identificacao,
                            emitente,
                            destinatario,
                            produtos,
                            pagamentos,
                            request.NotaFiscal.ValorTotalProdutos,
                            request.NotaFiscal.ProtocoloAutorizacao,
                            request.NotaFiscal.DataHoraAutorização,
                            infoAdicionalComplementar,
                            request.NotaFiscal.QtdTotalProdutos,
                            veiculo,
                            transportadora,
                            calculoImposto);


                    DgSystem.NFe.Reports.Nfe.DanfeNfe.GerarPDFNfe(notaFiscal);
                }
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                log.Error("Erro ao criar danfe.", e);
                return Task.FromResult(false);
            }
        }

        public Task<string> Handle(global::NFe.Core.Domain.GerarDanfeNfceEmail request, CancellationToken cancellationToken)
        {
            Identificacao identificacao
                = new Identificacao(
                    new Chave(request.NotaFiscal.Identificacao.Chave.ChaveMasked),
                    request.NotaFiscal.Identificacao.Numero,
                    request.NotaFiscal.Identificacao.Serie.ToString(),
                    request.NotaFiscal.Identificacao.DataHoraEmissao,
                    request.NotaFiscal.Identificacao.LinkConsultaChave,
                    request.NotaFiscal.Identificacao.MensagemInteresseContribuinte);

            Emitente emitente
                = new Emitente(
                    request.NotaFiscal.Emitente.CNPJ,
                    request.NotaFiscal.Emitente.Endereco.Logradouro,
                    request.NotaFiscal.Emitente.Nome,
                    request.NotaFiscal.Emitente.Endereco.Numero,
                    request.NotaFiscal.Emitente.Endereco.Bairro,
                    request.NotaFiscal.Emitente.Endereco.Municipio,
                    request.NotaFiscal.Emitente.Endereco.UF,
                    request.NotaFiscal.Emitente.Endereco.Cep);

            Destinatario destinatario
                = new Destinatario(
                    request.NotaFiscal.Destinatario.NomeRazao,
                    request.NotaFiscal.Destinatario.Documento.Numero,
                    request.NotaFiscal.Destinatario.Endereco?.Logradouro,
                    request.NotaFiscal.Destinatario.Endereco?.Numero,
                    request.NotaFiscal.Destinatario.Endereco?.Bairro,
                    request.NotaFiscal.Destinatario.Endereco?.Municipio,
                    request.NotaFiscal.Destinatario.Endereco?.UF,
                    request.NotaFiscal.Destinatario.Endereco?.Cep);

            IEnumerable<Produto> produtos =
                request.NotaFiscal.Produtos
                .Select(p =>
                    new Produto(
                        p.Codigo,
                        p.Descricao,
                        p.ValorUnidadeComercial,
                        p.ValorTotal,
                        p.QtdeUnidadeComercial,
                        p.Desconto,
                        p.Frete,
                        p.Seguro,
                        p.Outros));

            IEnumerable<Pagamento> pagamentos =
                request.NotaFiscal.Pagamentos
                .Select(p =>
                    new Pagamento(p.FormaPagamentoTexto, p.Valor));

            var notaFiscal
                = new DgSystem.NFe.Reports.Nfce.NotaFiscal(
                    request.NotaFiscal.QrCodeUrl,
                    identificacao,
                    emitente,
                    destinatario,
                    produtos,
                    pagamentos,
                    request.NotaFiscal.ValorTotalProdutos,
                    request.NotaFiscal.ProtocoloAutorizacao,
                    request.NotaFiscal.DataHoraAutorização,
                    request.NotaFiscal.InfoAdicional.InfoAdicionalComplementar,
                    request.NotaFiscal.QtdTotalProdutos);

            return Task.FromResult(DanfeNfce.ObterPdfEnvioNotaFiscalEmail(notaFiscal));
        }
    }
}
