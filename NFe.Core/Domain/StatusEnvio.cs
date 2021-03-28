using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;

namespace NFe.Core.NotaFiscal
{
    public class StatusEnvio
    {
        private readonly Status _status;

        public StatusEnvio(Status status)
        {
            _status = status;
        }

        public int GetIntValue()
        {
            return (int) _status;
        }

        public Status Status
        {
            get
            {
                return _status;
            }
        }

        public override string ToString()
        {
            switch (_status)
            {
                case Status.ENVIADA:
                    return "Enviada";
                case Status.CONTINGENCIA:
                    return "Contingência";
                case Status.PENDENTE:
                    return "Pendente";
                case Status.CANCELADA:
                    return "Cancelada";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsContingencia()
        {
            return _status == Status.CONTINGENCIA;
        }

        public bool IsCancelada()
        {
            return _status == Status.CANCELADA;
        }
    }
}
