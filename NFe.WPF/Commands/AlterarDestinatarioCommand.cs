using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmissorNFe.Model;
using MediatR;

namespace NFe.WPF.Commands
{
    public class AlterarDestinatarioCommand : IRequest
    {
        public DestinatarioModel DestinatarioSelecionado { get; set; }

        public AlterarDestinatarioCommand(DestinatarioModel destSelecionado)
        {
            this.DestinatarioSelecionado = destSelecionado;
        }
    }
}
