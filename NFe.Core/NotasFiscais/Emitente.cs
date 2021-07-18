using NFe.Core.Domain;
using System;
using System.Text.RegularExpressions;

namespace NFe.Core.Domain
{
    public class Emissor
    {
        public Emissor()
        {
        }

        public Emissor(string nome, string nomeFantasia, string cnpj, string inscricaoEstadual,
            string inscricaoMunicipal, string cnae, string regimeTributario, Endereco endereco, string telefone)
        {
            Nome = nome;
            NomeFantasia = nomeFantasia;
            CNPJ = cnpj;
            InscricaoEstadual = inscricaoEstadual;
            InscricaoMunicipal = inscricaoMunicipal;
            CNAE = cnae;
            CRT = Crt.Parse(Regex.Replace(regimeTributario, @"\s+", ""));
            Endereco = endereco;
            Telefone = telefone;
        }

        public string Nome { get; set; }
        public string NomeFantasia { get; set; }
        public string CNPJ { get; set; }
        public string InscricaoEstadual { get; set; }
        public string InscricaoMunicipal { get; set; }
        public string CNAE { get; set; }
        public Crt CRT { get; set; }
        public Endereco Endereco { get; set; }
        public string Telefone { get; set; }
    }
}