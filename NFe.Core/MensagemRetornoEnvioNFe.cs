using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;

namespace NFe.Core
{
    public enum TipoMensagem
    {
        Sucesso,
        Erro,
        ErroValidacao,
        SemConexao,
        ServicoIndisponivel,
        ErroValidacaoSEFAZ,
        RetornoEmissaoContingencia
    }

    public class MensagemRetornoEnvioNFe
    {
        public TipoMensagem TipoMensagem { get; set; }
        public string Mensagem { get; set; }
        public RetornoNotaFiscal Retorno { get; set; }
        public int IdNotaCopiaSeguranca { get; internal set; }
    }
}