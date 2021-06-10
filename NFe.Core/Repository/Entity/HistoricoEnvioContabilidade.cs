using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
   [Table("HistoricoEnvioContabilidade")]
   public class HistoricoEnvioContabilidade
   {
      public int Id { get; set; }
      [Required]
      public DateTime DataEnvio { get; set; }
      [Required]
      public string Periodo { get; set; }
   }
}
