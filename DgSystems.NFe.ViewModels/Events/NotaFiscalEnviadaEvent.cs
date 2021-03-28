
using NFe.Core.NotaFiscal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class NotaFiscalEnviadaEvent 
    {
        public NFe.Core.NotaFiscal.NotaFiscal NotaFiscal { get; internal set; }
    }
}
