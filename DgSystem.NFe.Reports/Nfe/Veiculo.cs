using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystem.NFe.Reports.Nfe
{
    public class Veiculo
    {
        public Veiculo(string placa, string siglaUF, string registroAntt)
        {
            Placa = placa;
            SiglaUF = siglaUF;
            RegistroAntt = registroAntt;
        }

        public string Placa { get;  }
        public string SiglaUF { get;  }
        public string RegistroAntt { get;  }
    }
}
