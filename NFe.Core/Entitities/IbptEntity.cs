using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("Ibpt")]
    public partial class IbptEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(8)]
        public string NCM { get; set; }

        [Required]
        [StringLength(100)]
        public string Descricao { get; set; }

        [Required]
        public double TributacaoFederal { get; set; }

        [Required]
        public double TributacaoEstadual { get; set; }
    }
}
