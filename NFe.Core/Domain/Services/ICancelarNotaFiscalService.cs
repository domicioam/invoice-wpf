using System.Threading.Tasks;
using NFe.Core.Domain;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Sefaz.Facades;

namespace NFe.Core.NotasFiscais.Services
{
    public interface ICancelaNotaFiscalService
    {
        MensagemRetornoEventoCancelamento CancelarNotaFiscal(DadosNotaParaCancelar dadosNotaParaCancelar, string justificativa);
    }
}