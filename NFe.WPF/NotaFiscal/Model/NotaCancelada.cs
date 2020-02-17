using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmissorNFe.Utils
{
    public class NotaCancelada
    {
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string DataEmissao { get; set; }
        public string DataCancelamento { get; set; }
        public string ProtocoloCancelamento { get; set; }
        public string MotivoCancelamento { get; set; }
    }
}
