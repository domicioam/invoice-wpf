using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Destinatario
{
    [Table("EnderecoDestinatario")]
    public class EnderecoDestinatarioEntity
    {
        [Key, ForeignKey("Destinatario")]
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
        public virtual DestinatarioEntity Destinatario { get; set; }
    }
}
