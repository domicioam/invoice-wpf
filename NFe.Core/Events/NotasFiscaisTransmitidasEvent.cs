using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Events
{
    public class NotasFiscaisTransmitidasEvent : IRequest
    {
        public List<string> MensagensErro { get; set; }
    }
}
