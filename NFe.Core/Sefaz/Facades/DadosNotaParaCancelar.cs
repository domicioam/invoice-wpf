using NFe.Core.NotaFiscal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Sefaz.Facades
{
    public class DadosNotaParaCancelar
    {
        public string ufEmitente { get; set; }
        public CodigoUfIbge codigoUf { get; set; }
        public string cnpjEmitente { get; set; }
        public string chaveNFe { get; set; }
        public string protocoloAutorizacao { get; set; }
        public Modelo modeloNota { get; set; }
    }
}
