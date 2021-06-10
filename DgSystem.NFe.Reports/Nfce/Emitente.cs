using System;

namespace DgSystem.NFe.Reports.Nfce
{
    [Serializable]
    public class Emitente
    {
        public Emitente(string cNPJ, string logradouro, string nome, string numero, string bairro, string municipio, string uF, string cEP)
        {
            CNPJ = cNPJ;
            Logradouro = logradouro;
            Nome = nome;
            Numero = numero;
            Bairro = bairro;
            Municipio = municipio;
            UF = uF;
            CEP = cEP;
        }

        public string CNPJ { get; }
        public string Logradouro { get; }
        public string Nome { get; }
        public string Numero { get; }
        public string Bairro { get; }
        public string Municipio { get; }
        public string UF { get; }
        public string CEP { get; }
    }
}