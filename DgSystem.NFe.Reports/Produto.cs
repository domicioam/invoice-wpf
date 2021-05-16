namespace DgSystem.NFe.Reports
{
    public class Produto
    {
        public object Codigo { get; internal set; }
        public object Descricao { get; internal set; }
        public object ValorUnitario { get; internal set; }
        public object ValorTotal { get; internal set; }
        public object Quantidade { get; internal set; }
        public int Desconto { get; internal set; }
        public int Frete { get; internal set; }
        public int Seguro { get; internal set; }
        public int Outros { get; internal set; }
    }
}