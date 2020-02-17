using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("Transportadora")]
    public class TransportadoraEntity
    {
        public int Id { get; set; }
        public bool IsPessoaJuridica { get; set; }
        public string NomeRazao { get; set; }
        [StringLength(14)]
        [Required]
        public string CpfCnpj { get; set; }
        [Required]
        public string InscricaoEstadual { get; set; }
        public virtual EnderecoTransportadoraEntity Endereco { get; set; }
    }
}