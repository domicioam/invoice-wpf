using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("CFOP")]
    public class CfopEntity
    {
        public int Id { get; set; }
        [Required]
        [StringLength(4)]
        public string Cfop { get; set; }
        [Required]
        [StringLength(150)]
        public string Descricao { get; set; }
        public int NaturezaOperacaoId { get; set; }
        public virtual NaturezaOperacaoEntity NaturezaOperacao { get; set; }
    }
}
