using NFe.Core.Cadastro.Imposto;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais.Repositories
{
    public static class TorigExtensions
    {

        public static Origem ToOrigem(this Torig torig)
        {
            Origem? origem = null;
            switch (torig)
            {
                case Torig.Item0:
                    origem = Origem.Nacional;
                    break;
                case Torig.Item1:
                case Torig.Item2:
                case Torig.Item3:
                case Torig.Item4:
                case Torig.Item5:
                case Torig.Item6:
                case Torig.Item7:
                case Torig.Item8:
                    throw new NotSupportedException(torig.ToString());
            }

            return origem.Value;
        }
    }
}
