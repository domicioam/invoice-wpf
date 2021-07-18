using System;
using NFe.Core.Domain;
using NFe.Core.Utils.Acentuacao;
using NFe.Core.Utils.Conversores;

namespace NFe.Core.Domain
{
    public class Endereco
    {
        private string _municipio;
        private string _uf;

        public Endereco(string logradouro, string numero, string bairro, string municipio, string cep, string uf)
        {
            Bairro = bairro;
            Cep = cep;
            Logradouro = logradouro;
            Municipio = municipio;
            Numero = numero;
            UF = uf;
        }

        public Endereco()
        {

        }

        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string CodigoMunicipio { get; private set; }

        public string UF
        {
            get { return _uf; }
            set
            {
                _uf = value;
                CodigoUF = UfToCodigoUfConversor.GetCodigoUf(_uf);
            }
        }

        public string Cep { get; set; }

        public string Pais
        {
            get { return "Brasil"; }
        }

        public string Municipio
        {
            get { return _municipio; }
            set
            {
                _municipio = value;
                SetCodigoMunicipio(_municipio);
            }
        }

        public string CodigoUF { get; private set; }

        private void SetCodigoMunicipio(string municipio)
        {
            var municipioSemAcentos = Acentuacao.RemoverAcentuacao(municipio).ToUpper();

            CodigoMunicipio = ((int) Enum.Parse(typeof(CodMunicipioIBGE), municipioSemAcentos)).ToString();
        }
    }
}