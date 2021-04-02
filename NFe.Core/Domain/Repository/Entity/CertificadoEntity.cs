using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Certificado
{
    [Table("Certificado")]
    public class CertificadoEntity
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Nome { get; set; }

        [StringLength(260)]
        public string Caminho { get; set; }

        [StringLength(50)]
        [Required]
        public string NumeroSerial { get; set; }

        [StringLength(50)]
        public string Senha { get; set; }
    }
}
