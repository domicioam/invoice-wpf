using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Utils.PDF.RelatorioGerencial
{
    public class NotaFiscal
    {

        public string Modelo { get; set; }


        public string Serie { get; set; }


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

        public string TipoEmissao { get; set; }

        public string Protocolo { get; set; }
        public int Id { get; set; }
    }
}
