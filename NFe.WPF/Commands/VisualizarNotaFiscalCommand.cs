using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmissorNFe.Model;
using NFe.WPF.NotaFiscal.Model;


namespace NFe.WPF.Commands
{
    public class VisualizarNotaFiscalCommand 
    {
        public NFCeModel NotaFiscal { get; set; }

        public VisualizarNotaFiscalCommand(NFCeModel notaFiscal)
        {
            this.NotaFiscal = notaFiscal;
        }
    }
}
