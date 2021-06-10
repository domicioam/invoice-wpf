using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Cadastro.Configuracoes
{
    [Table("Configuracao")]
    public partial class ConfiguracaoEntity
    {
        public int Id { get; set; }

        [StringLength(8)]
        public string SerieNFe { get; set; }

        [StringLength(8)]
        public string ProximoNumNFe { get; set; }

        [StringLength(8)]
        public string SerieNFCe { get; set; }

        [StringLength(8)]
        public string ProximoNumNFCe { get; set; }

        [StringLength(6)]
        public string CscId { get; set; }

        [StringLength(36)]
        public string Csc { get; set; }

        [StringLength(50)]
        public string EmailContabilidade { get; set; }

        public bool IsContingencia { get; set; }
        public DateTime DataHoraEntradaContingencia { get; set; }
        public string JustificativaContingencia { get; set; }
    }
}
