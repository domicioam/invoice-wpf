
using NFe.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class NotaFiscalPendenteReenviadaEvent 
    {
        public NFe.Core.Domain.NotaFiscal NotaFiscal { get; internal set; }
    }
}
