using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    public enum Origem
    {
        Nacional
    }

    public enum TipoImposto
    {
        Confins,
        Icms,
        IcmsST,
        IPI,
        PIS
    }

    [Table("Imposto")]
    public class Imposto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public double BaseCalculo { get; set; }
        public double Aliquota { get; set; }
        public double Reducao { get; set; }
        public string CST { get; set; }
        public TipoImposto TipoImposto { get; set; }
        public Origem Origem { get; set; }
        public ProdutoEntity Produto { get; set; }
    }
}
