using EmissorNFe.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class DestinatarioSalvoEvent 
    {
        public DestinatarioSalvoEvent(DestinatarioModel destinatarioParaSalvar) : base()
        {
            this.Destinatario = destinatarioParaSalvar;
        }

        public DestinatarioSalvoEvent()
        {

        }

        public DestinatarioModel Destinatario { get; set; }
    }
}
