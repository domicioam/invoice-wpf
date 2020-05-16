using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Commands
{
    public class EnviarEmailCommand : IRequest
    {
        public string Chave { get; set; }

        public EnviarEmailCommand(string chave)
        {
            this.Chave = chave;
        }
    }
}
