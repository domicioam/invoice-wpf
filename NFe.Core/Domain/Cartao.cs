namespace NFe.Core.NotaFiscal
{
    public enum Bandeira
    {
        Visa,
        Mastercard,
        AmericanExpress,
        Sorocred,
        Outros
    }

    public class Cartao
    {
        public string CnpjCredenciadora { get; set; }
        public Bandeira Bandeira { get; set; }
        public string NumeroAutorizacao { get; set; }
    }
}