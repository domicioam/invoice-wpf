using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;

namespace NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao
{
    internal class MensagemRetornoTransmissaoNotasContingencia
    {
        public TipoMensagem TipoMensagem { get; set; }
        public string Mensagem { get; set; }
        public TRetEnviNFeInfRec RetEnviNFeInfRec { get; set; }
    }
}