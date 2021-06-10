using DgSystem.NFe.Reports.Nfce;
using MediatR;
using NFe.Core;
using NFe.Core.Utils.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DgSystems.NFe.NotaFiscal.Reports
{
    public class ImprimirDanfeHandler : IRequestHandler<ImprimirDanfe, bool>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // send message from NotaFiscal bounded context to Reports bounded context
        public Task<bool> Handle(ImprimirDanfe request, CancellationToken cancellationToken)
        {
            // manda imprimir e responde com true se sucesso

            // if nfce
            // call DanfeNfce
            // move qrcode generation to DgSystems.NFe.Reports

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
                = new DgSystem.NFe.Reports.NotaFiscal(
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

            try
            {
                if (request.NotaFiscal.Identificacao.Modelo == global::NFe.Core.Domain.Modelo.Modelo65)
                {
                    DanfeNfce.GerarPDFNfce(notaFiscal);
                }
                else
                {
                    // DanfeNfe
                }
                return Task.FromResult(true);
            }
            catch(Exception e)
            {
                log.Error($"{nameof(ImprimirDanfeHandler)}: Erro ao criar danfe.", e);
                return Task.FromResult(false);
            }
        }
    }
}
