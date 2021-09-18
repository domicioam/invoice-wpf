using NFe.Core.Domain;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;

namespace NFe.Core.Sefaz.Facades
{
    public class RetornoInutilizacao
    {
        public Status Status { get; set; }
        public string Mensagem { get; set; }
        public string Xml { get; set; }
        public string DataInutilizacao { get; set; }
        public string IdInutilizacao { get; set; }
        public string ProtocoloInutilizacao { get; set; }
        public string MotivoInutilizacao { get; set; }
    }

    public interface IInutilizarNotaFiscalService
    {
        RetornoInutilizacao InutilizarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, Modelo modeloNota, string serie, string numeroInicial, string numeroFinal);
    }
}