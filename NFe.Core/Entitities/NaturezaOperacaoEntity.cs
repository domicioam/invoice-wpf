using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("NaturezaOperacao")]
    public class NaturezaOperacaoEntity
    {
        public NaturezaOperacaoEntity()
        {
            Cfops = new List<CfopEntity>();
        }

        public int Id { get; set; }

        [StringLength(20)]
        [Required]
        public string Descricao { get; set; }

        public virtual ICollection<CfopEntity> Cfops { get; set; }
    }
}
