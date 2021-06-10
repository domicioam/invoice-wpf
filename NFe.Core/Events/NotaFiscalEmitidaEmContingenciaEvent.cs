
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Events
{
    public class NotaFiscalEmitidaEmContingenciaEvent 
    {
        public string justificativa { get; set; }
        public DateTime horário { get; set; }
    }
}
