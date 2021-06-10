using System;

namespace DgSystem.NFe.Reports.Nfe
{
    [Serializable]
    public class Emitente
    {
        public Emitente(string cNPJ, string inscricaoEstadual, string nomeRazao, string nomeFantasia, string logradouro, 
            string numero, string bairro, string municipio, string uF, string cEP, string telefone)
        {
            CNPJ = cNPJ;
            InscricaoEstadual = inscricaoEstadual;
            NomeRazao = nomeRazao;
            NomeFantasia = nomeFantasia;
            Logradouro = logradouro;
            Numero = numero;
            Bairro = bairro;
            Municipio = municipio;
            UF = uF;
            CEP = cEP;
            Telefone = telefone;
        }

        public string CNPJ { get; set; }
        public string InscricaoEstadual { get; set; }
        public string NomeRazao { get; set; }
        public string NomeFantasia { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string CEP { get; set; }
        public string Telefone { get; set; }
    }
}