using EmissorNFe.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class NotaFiscalInutilizadaEvent 
    {
        public NFCeModel NotaFiscal { get; internal set; }
    }
}
