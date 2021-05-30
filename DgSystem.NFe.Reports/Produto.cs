namespace DgSystem.NFe.Reports
{
    public class Produto
    {
        public string Codigo { get; internal set; }
        public string Descricao { get; internal set; }
        public double ValorUnitario { get; internal set; }
        public double ValorTotal { get; internal set; }
        public int Quantidade { get; internal set; }
        public int Desconto { get; internal set; }
        public int Frete { get; internal set; }
        public int Seguro { get; internal set; }
        public int Outros { get; internal set; }
    }
}