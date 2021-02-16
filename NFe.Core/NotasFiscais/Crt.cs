using NFe.Core.Extensions;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    public class Crt : Enumeration
    {
        public static readonly Crt SimplesNacional = new Crt(0, "SimplesNacional");
        public static readonly Crt SimplesNacionalExcessoReceitaBruta = new Crt(1, "SimplesNacionalExcessoReceitaBruta");
        public static readonly Crt RegimeNormal = new Crt(2, "RegimeNormal");

        public Crt() { }

        private Crt(int value, string displayName) : base(value, displayName)
        {
        }

        public static implicit operator TNFeInfNFeEmitCRT(Crt crt)
        {
            if(crt == Crt.SimplesNacional)
            {
                return TNFeInfNFeEmitCRT.Item1;
            } else if(crt == Crt.SimplesNacionalExcessoReceitaBruta)
            {
                return TNFeInfNFeEmitCRT.Item2;
            }
            else if(crt == Crt.RegimeNormal)
            {
                return TNFeInfNFeEmitCRT.Item3;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static Crt Parse(string name)
        {
            return FromDisplayName<Crt>(name);
        }
    }
}
