using System;
using System.Security.Cryptography.X509Certificates;
using NFe.Core.Domain;

namespace NFe.Core.NotasFiscais.Sefaz.NfeConsulta2
{
    public struct MensagemRetornoConsulta
    {
        public bool IsEnviada { get; set; }
        public DateTime DhAutorizacao { get; set; }
        public Protocolo Protocolo { get; set; }
    }

    public interface IConsultarNotaFiscalService
    {
        MensagemRetornoConsulta ConsultarNotaFiscal(string chave, string codigoUf, X509Certificate2 certificado, Modelo modelo);
    }
}