using EmissorNFe.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.WPF.NotaFiscal.Model;
using DgSystems.NFe.ViewModels;

namespace NFe.WPF.Events
{
    public class NotaFiscalInutilizadaEvent 
    {
        public NFCeViewModel NotaFiscal { get; internal set; }
    }
}
