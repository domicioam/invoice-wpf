using System.Threading.Tasks;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;

namespace NFe.Core.NotasFiscais.Services
{
    public interface ICancelaNotaFiscalFacade
    {
        MensagemRetornoEventoCancelamento CancelarNotaFiscal(string ufEmitente,
            CodigoUfIbge codigoUf, string cnpjEmitente, string chaveNFe,
            string protocoloAutorizacao, Modelo modeloNota, string justificativa);
    }
}