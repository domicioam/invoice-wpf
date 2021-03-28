using NFe.Core.Domain;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;

namespace NFe.Core.Sefaz.Facades
{
    public interface IInutilizarNotaFiscalService
    {
        MensagemRetornoInutilizacao InutilizarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, Modelo modeloNota, string serie, string numeroInicial, string numeroFinal);
    }
}