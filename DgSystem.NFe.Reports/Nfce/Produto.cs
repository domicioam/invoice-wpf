using System;

namespace DgSystem.NFe.Reports.Nfce
{
    [Serializable]
    public class Produto
    {
        public Produto(string codigo, string descricao, double valorUnitario, double valorTotal, int quantidade, double desconto, double frete, double seguro, double outros)
        {
            Codigo = codigo;
            Descricao = descricao;
            ValorUnitario = valorUnitario;
            ValorTotal = valorTotal;
            Quantidade = quantidade;
            Desconto = desconto;
            Frete = frete;
            Seguro = seguro;
            Outros = outros;
        }

        public string Codigo { get; }
        public string Descricao { get; }
        public double ValorUnitario { get; }
        public double ValorTotal { get; }
        public int Quantidade { get; }
        public double Desconto { get; }
        public double Frete { get; }
        public double Seguro { get; }
        public double Outros { get; }
    }
}