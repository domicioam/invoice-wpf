using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFe.Core.Entitities
{
    public delegate void AmbienteAlteradoEventHandler();

    [Table("Configuracao")]
    public partial class ConfiguracaoEntity
    {
        [NotMapped]
        public AmbienteAlteradoEventHandler AmbienteAlteradoEvent = delegate { };

        private bool _isProducao;

        [Required]
        public bool IsProducao
        {
            get { return _isProducao; }
            set
            {
                _isProducao = value;
                AmbienteAlteradoEvent();
            }
        }

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

        [StringLength(8)]
        public string SerieNFeHom { get; set; }

        [StringLength(8)]
        public string ProximoNumNFeHom { get; set; }

        [StringLength(8)]
        public string SerieNFCeHom { get; set; }

        [StringLength(8)]
        public string ProximoNumNFCeHom { get; set; }

        [StringLength(6)]
        public string CscIdHom { get; set; }

        [StringLength(36)]
        public string CscHom { get; set; }

        [StringLength(50)]
        public string EmailContabilidadeHom { get; set; }
        public bool IsContingencia { get; set; }
        public DateTime DataHoraEntradaContingencia { get; set; }
        public string JustificativaContingencia { get; set; }
    }
}
