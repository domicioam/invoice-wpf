using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Imposto
{
    [Table("GrupoImpostos")]
    public partial class GrupoImpostos
    {
        public GrupoImpostos()
        {
            Impostos = new HashSet<Entitities.Imposto>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CFOP { get; set; }

        [Required]
        [StringLength(30)]
        public string Descricao { get; set; }

        public virtual ICollection<Entitities.Imposto> Impostos { get; set; }
    }
}
