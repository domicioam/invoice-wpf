using System.Security.Cryptography.X509Certificates;
using NFe.Core.Domain;

namespace NFe.Core.NotasFiscais.Sefaz.NfeConsulta2
{
    public interface IConsultarNotaFiscalService
    {
        NFeConsulta.MensagemRetornoConsulta ConsultarNotaFiscal(string chave, string codigoUf, X509Certificate2 certificado, Modelo modelo);
    }
}