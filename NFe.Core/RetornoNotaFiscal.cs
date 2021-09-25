using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;

namespace NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao
{
    public class RetornoNotaFiscal
   {
      public string CodigoStatus { get; set; }
      public string Chave { get; set; }
      public string Motivo { get; set; }
      public string UrlQrCode { get; set; }
      public Protocolo Protocolo { get; set; }
      public string DataAutorizacao { get; set; }
      public string Xml { get; set; }
   }
}
