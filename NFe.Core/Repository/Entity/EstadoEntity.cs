using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("Estado")]
    public class EstadoEntity
    {
        public int Id { get; set; }
        public int CodigoUf { get; set; }
        public string Nome { get; set; }
        [StringLength(2)]
        public string Uf { get; set; }
        public int Regiao { get; set; }
    }
}
