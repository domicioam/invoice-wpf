using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("Emitente")]
    public partial class EmitenteEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RazaoSocial { get; set; }

        [Required]
        [StringLength(14)]
        public string CNPJ { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeFantasia { get; set; }

        [Required]
        [StringLength(13)]
        public string InscricaoEstadual { get; set; }

        [Required]
        [StringLength(13)]
        public string InscricaoMunicipal { get; set; }

        [Required]
        [StringLength(50)]
        public string RegimeTributario { get; set; }

        [Required]
        [StringLength(7)]
        public string CNAE { get; set; }

        [Required]
        [StringLength(10)]
        public string CEP { get; set; }

        [Required]
        [StringLength(100)]
        public string Logradouro { get; set; }

        [Required]
        [StringLength(50)]
        public string Bairro { get; set; }

        [Required]
        [StringLength(2)]
        public string UF { get; set; }

        [Required]
        [StringLength(50)]
        public string Municipio { get; set; }

        [StringLength(50)]
        public string Telefone { get; set; }

        [StringLength(50)]
        public string Contato { get; set; }
        [StringLength(10)]
        public string Numero { get; set; }
    }
}
