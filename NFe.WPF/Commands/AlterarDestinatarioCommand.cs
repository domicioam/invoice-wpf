using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmissorNFe.Model;


namespace NFe.WPF.Commands
{
    public class AlterarDestinatarioCommand 
    {
        public DestinatarioModel DestinatarioSelecionado { get; set; }

        public AlterarDestinatarioCommand(DestinatarioModel destSelecionado)
        {
            this.DestinatarioSelecionado = destSelecionado;
        }
    }
}
