namespace NFe.Core.Entitities
{
    public partial class ProdutoNotaFiscal
    {
        public int Quantidade { get; }

        public string Codigo { get; }

        public string Descricao { get; }

        public double ValorUnitario { get; }

        public string UnidadeComercial { get; }

        public string NCM { get; }

        public ProdutoNotaFiscal(int quantidade, string codigo, string descri��o, double valorUnit�rio, string unidadeComercial, string ncm)
        {
            Quantidade = quantidade;
            Codigo = codigo;
            Descricao = descri��o;
            ValorUnitario = valorUnit�rio;
            UnidadeComercial = unidadeComercial;
            NCM = ncm;
        }
    }
}
