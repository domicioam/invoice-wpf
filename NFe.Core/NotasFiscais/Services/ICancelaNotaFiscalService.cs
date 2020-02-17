using System.Threading.Tasks;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;

namespace NFe.Core.NotasFiscais.Services
{
    public interface ICancelaNotaFiscalService
    {
        MensagemRetornoEventoCancelamento CancelarNotaFiscal(string ufEmitente,
            CodigoUfIbge codigoUf, Ambiente ambiente, string cnpjEmitente, string chaveNFe,
            string protocoloAutorizacao, Modelo modeloNota, string justificativa);
    }
}