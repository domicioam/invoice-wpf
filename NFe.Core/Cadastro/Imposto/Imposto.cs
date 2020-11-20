using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Imposto
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
        public int GrupoImpostosId { get; set; }
        public double BaseCalculo { get; set; }
        public double Aliquota { get; set; }
        public double Reducao { get; set; }
        public string CST { get; set; }
        public TipoImposto TipoImposto { get; set; }
        public Origem Origem { get; set; }
        public GrupoImpostos GrupoImpostos { get; set; }
    }

    internal class ImpostoIdComparer : IEqualityComparer<Imposto>
    {
        public bool Equals(Imposto x, Imposto y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Imposto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
