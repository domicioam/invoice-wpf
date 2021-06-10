using NFe.Core.Cadastro.Destinatario;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("EnderecoDestinatario")]
    public class EnderecoEntity
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
