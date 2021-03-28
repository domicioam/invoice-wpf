using NFe.Core.Domain;

namespace NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento
{
    public interface INFeCancelamento
    {
        MensagemRetornoEventoCancelamento CancelarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, string chaveNFe, string protocoloAutorizacao, Modelo modeloNota, string justificativa);
    }
}