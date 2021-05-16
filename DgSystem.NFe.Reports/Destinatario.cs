namespace DgSystem.NFe.Reports
{
    public class Destinatario
    {
        public Destinatario(string v)
        {
            V = v;
        }

        public string V { get; }
        public object Documento { get; internal set; }
        public object Logradouro { get; internal set; }
        public object Numero { get; internal set; }
        public object Bairro { get; internal set; }
        public object Municipio { get; internal set; }
        public object UF { get; internal set; }
        public object CEP { get; internal set; }
        public string NomeRazao { get; internal set; }
    }
}