using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NFe.Interfaces;

namespace NFe.Core.Entitities
{
    public enum Status
    {
        ENVIADA,
        PENDENTE,
        CANCELADA,
        CONTINGENCIA
    }

    [Table("NotaFiscal")]
    public partial class NotaFiscalEntity : IXmlFileWritable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NotaFiscalEntity()
        {
        }

        public int Id { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [StringLength(2)]
        public string Modelo { get; set; }

        [Required]
        [StringLength(44)]
        public string Chave { get; set; }

        [Required]
        [StringLength(3)]
        public string Serie { get; set; }

        [Required]
        public int Ambiente { get; set; }

        [Required]
        public string Numero { get; set; }

        public DateTime DataEmissao { get; set; }
        private DateTime _dataAutorizacao = DateTime.Now;
        public DateTime DataAutorizacao
        {
            get { return _dataAutorizacao; }
            set { _dataAutorizacao = value; }
        }

        public double ValorProdutos { get; set; }

        public double ValorServicos { get; set; }

        public double ValorICMSST { get; set; }

        public double ValorFrete { get; set; }

        public double ValorSeguro { get; set; }

        public double ValorIPI { get; set; }

        public double ValorDespesas { get; set; }

        public double ValorDesconto { get; set; }

        public double ValorISS { get; set; }

        public double ValorICMS { get; set; }

        public double ValorTotal { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoEmissao { get; set; }

        [StringLength(20)]
        public string Protocolo { get; set; }

        [Required]
        public string XmlPath { get; set; }

        [Required]
        public string Destinatario { get; set; }
        [Required]
        public string UfDestinatario { get; set; }

        public string DocumentoDestinatario { get; set; }
        [Required]
        public bool IsProducao { get; set; }

        [NotMapped]
        public string FileName
        {
            get
            {
                if (Modelo.Equals("65"))
                {
                    return Chave + "-nfce.xml";
                }
                else
                {
                    return Chave + "-nfe.xml";
                }
            }
        }
    }
}
