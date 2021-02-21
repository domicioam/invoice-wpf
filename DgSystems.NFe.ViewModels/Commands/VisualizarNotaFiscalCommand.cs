using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DgSystems.NFe.ViewModels;
using EmissorNFe.Model;
using NFe.WPF.NotaFiscal.Model;


namespace NFe.WPF.Commands
{
    public class VisualizarNotaFiscalCommand 
    {
        public NFCeViewModel NotaFiscal { get; set; }

        public VisualizarNotaFiscalCommand(NFCeViewModel notaFiscal)
        {
            this.NotaFiscal = notaFiscal;
        }
    }
}
