using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Imposto
{
    [Table("GrupoImpostos")]
    public class GrupoImpostos
    {
        public GrupoImpostos()
        {
            Impostos = new HashSet<Imposto>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CFOP { get; set; }

        [Required]
        [StringLength(30)]
        public string Descricao { get; set; }

        public virtual ICollection<Imposto> Impostos { get; set; }
    }
}
