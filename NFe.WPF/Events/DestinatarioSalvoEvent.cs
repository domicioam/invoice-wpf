using EmissorNFe.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class DestinatarioSalvoEvent : INotification
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
