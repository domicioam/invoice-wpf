using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("Produto")]
    public partial class ProdutoEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProdutoEntity()
        {
        }

        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(30)]
        public string Descricao { get; set; }

        public int GrupoImpostosId { get; set; }

        public double ValorUnitario { get; set; }

        [Required]
        [StringLength(2)]
        public string UnidadeComercial { get; set; }

        [Required]
        [StringLength(20)]
        public string NCM { get; set; }

        public virtual GrupoImpostos GrupoImpostos { get; set; }
    }
}
