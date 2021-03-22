using System.Security.Cryptography.X509Certificates;

namespace NFe.Core.NotasFiscais.Sefaz.NfeConsulta2
{
    public interface INFeConsulta
    {
        NFeConsulta.MensagemRetornoConsulta ConsultarNotaFiscal(string chave, string codigoUf, X509Certificate2 certificado, Modelo modelo);
    }
}