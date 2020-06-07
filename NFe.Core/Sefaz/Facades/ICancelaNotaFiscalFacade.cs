using System.Threading.Tasks;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Sefaz.Facades;

namespace NFe.Core.NotasFiscais.Services
{
    public interface ICancelaNotaFiscalFacade
    {
        MensagemRetornoEventoCancelamento CancelarNotaFiscal(DadosNotaParaCancelar dadosNotaParaCancelar, string justificativa);
    }
}