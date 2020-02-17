using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("EnderecoTransportadora")]
    public class EnderecoTransportadoraEntity
    {
        [Key, ForeignKey("Transportadora")]
        public int Id { get; set; }
        [StringLength(60)]
        [Required]
        public string Logradouro { get; set; }
        [StringLength(60)]
        [Required]
        public string Numero { get; set; }
        [StringLength(60)]
        [Required]
        public string Bairro { get; set; }
        [StringLength(2)]
        [Required]
        public string UF { get; set; }
        [StringLength(60)]
        [Required]
        public string Municipio { get; set; }
        public virtual TransportadoraEntity Transportadora { get; set; }
    }
}
