using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmissorNFe.Model;
using NFe.WPF.ViewModel;

namespace NFe.WPF.Commands
{
    public class AlterarDestinatarioCommand 
    {
        public DestinatarioViewModel DestinatarioViewModel { get; set; }

        public AlterarDestinatarioCommand(DestinatarioViewModel destinatarioViewModel)
        {
            this.DestinatarioViewModel = destinatarioViewModel;
        }
    }
}
