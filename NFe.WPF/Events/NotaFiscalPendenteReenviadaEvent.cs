
using NFe.Core.NotasFiscais;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class NotaFiscalPendenteReenviadaEvent 
    {
        public Core.NotasFiscais.NotaFiscal NotaFiscal { get; internal set; }
    }
}
