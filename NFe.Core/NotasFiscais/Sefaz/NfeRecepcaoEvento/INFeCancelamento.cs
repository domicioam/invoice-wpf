namespace NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento
{
    public interface INFeCancelamento
    {
        MensagemRetornoEventoCancelamento CancelarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, Ambiente ambiente, string cnpjEmitente, string chaveNFe, string protocoloAutorizacao, Modelo modeloNota, string justificativa);
    }
}