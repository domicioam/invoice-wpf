using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Destinatario
{
    [Table("Destinatario")]
    public class DestinatarioEntity
    {
        public int Id { get; set; }
        public string NomeRazao { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public virtual EnderecoDestinatarioEntity Endereco { get; set; }
        public string InscricaoEstadual { get; set; }
        [Required]
        [StringLength(20)]
        public string Documento { get; set; }
        [Required]
        public int TipoDestinatario { get; set; }
    }
}
