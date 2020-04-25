namespace NFe.Core.NotasFiscais
{
    public class Transportadora
    {
        public Transportadora(string cpfCnpj, string enderecoCompleto, string inscricaoEstadual, string municipio,
            string UF, string nomeRazao)
        {
            CpfCnpj = cpfCnpj;
            EnderecoCompleto = enderecoCompleto;
            InscricaoEstadual = inscricaoEstadual;
            Municipio = municipio;
            SiglaUF = UF;
            Nome = nomeRazao;
        }

        public string Nome { get; set; }
        public string CpfCnpj { get; set; }
        public string InscricaoEstadual { get; set; }
        public string EnderecoCompleto { get; set; }
        public string Municipio { get; set; }
        public string SiglaUF { get; set; }
    }
}