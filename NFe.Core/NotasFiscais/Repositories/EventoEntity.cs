using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NFe.Interfaces;

namespace NFe.Core.Entitities
{
    [Table("Evento")]
    public class EventoEntity : IXmlFileWritable
    {
        public int Id { get; set; }

        [Required]
        public string TipoEvento { get; set; }

        [Required]
        public DateTime DataEvento { get; set; }

        [Required]
        public string XmlPath { get; set; }
        [Required]
        public int NotaId { get; set; }
        [Required]
        public string ChaveIdEvento { get; set; }
        public string MotivoCancelamento { get; set; }
        public string ProtocoloCancelamento { get; set; }

        [NotMapped]
        public string FileName
        {
            get
            {
                return ChaveIdEvento + "-procEventoNFe.xml";
            }
        }

        [NotMapped]
        public string Xml { get; set; }
    }
}
