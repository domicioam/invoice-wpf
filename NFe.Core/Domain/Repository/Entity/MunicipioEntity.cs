using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("Municipio")]
    public class MunicipioEntity
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nome { get; set; }
        [StringLength(2)]
        public string Uf { get; set; }
    }
}
