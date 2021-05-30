namespace DgSystem.NFe.Reports
{
    public class Destinatario
    {
        public Destinatario(string nomeRazao)
        {
            NomeRazao = nomeRazao;
        }

        public string Documento { get; internal set; }
        public string Logradouro { get; internal set; }
        public string Numero { get; internal set; }
        public string Bairro { get; internal set; }
        public string Municipio { get; internal set; }
        public string UF { get; internal set; }
        public string CEP { get; internal set; }
        public string NomeRazao { get; internal set; }
    }
}