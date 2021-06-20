using System;

namespace DgSystem.NFe.Reports.Nfce
{
    [Serializable]
    public class Destinatario
    {
        public Destinatario(string nomeRazao, string documento = null, string logradouro = null, string numero = null, string bairro = null, string municipio = null, string uF = null, string cEP = null)
        {
            Nome = nomeRazao;
            Documento = documento;
            Logradouro = logradouro;
            Numero = numero;
            Bairro = bairro;
            Municipio = municipio;
            UF = uF;
            CEP = cEP;
        }

        public string Documento { get; }
        public string Logradouro { get; }
        public string Numero { get; }
        public string Bairro { get; }
        public string Municipio { get; }
        public string UF { get; }
        public string CEP { get; }
        public string Nome { get; }
    }
}