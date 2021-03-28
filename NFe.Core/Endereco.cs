using NFe.Core.NotaFiscal;

namespace NFe.Core.NotasFiscais
{
    public class EnderecoDestinatario : Endereco
    {
        public EnderecoDestinatario(string logradouro, string numero, string bairro, string municipio, string cep, string uf)
            : base(logradouro, numero, bairro, municipio, cep, uf)
        {
            Logradouro = Logradouro;
            Numero = numero;
            Bairro = bairro;
            Municipio = municipio;
            Cep = cep;
            UF = uf;
        }

        public string Complemento { get; set; }

        public override string ToString()
        {
            return Logradouro + " " + Numero + " " + Bairro + " - " + Municipio + " / " + UF;
        }
    }
}