using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    [Table("NotaInutilizada")]
    public class NotaInutilizadaEntity
    {
        public int Id { get; set; }
        [Required]
        public string Serie { get; set; }
        [Required]
        public string Numero { get; set; }
        [Required]
        public int Modelo { get; set; }
        [Required]
        public bool IsProducao { get; set; }
        [Required]
        public DateTime DataInutilizacao { get; set; }
        [Required]
        public string IdInutilizacao { get; set; }
        [Required]
        public string Protocolo { get; set; }
        [Required]
        public string Motivo { get; set; }
        [Required]
        public string XmlPath { get; set; }
    }
}
