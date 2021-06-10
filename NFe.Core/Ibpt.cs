namespace NFe.Core.NotasFiscais
{
    public class Ibpt
    {
        public Ibpt(string ncm, string descricao, double tributacaoFederal, double tributacaoEstadual)
        {
            NCM = ncm;
            Descricao = descricao;
            TributacaoEstadual = tributacaoEstadual;
            TributacaoFederal = tributacaoFederal;
        }

        public string NCM { get; set; }
        public string Descricao { get; set; }
        public double TributacaoFederal { get; set; }
        public double TributacaoEstadual { get; set; }
    }
}