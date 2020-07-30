using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;

namespace NFe.Core.NotasFiscais.Entities
{
    /*
     * Domain model
     */
    public class Impostos
    {
        private readonly IList<NotasFiscais.Imposto> _impostos;

        public Impostos()
        {
            _impostos = new List<NotasFiscais.Imposto>();
        }

        public Impostos(IEnumerable<Imposto> impostos)
        {
            var impostoFactory = new ImpostoFactory();

            foreach (var imposto in impostos)
            {
                var newImposto = impostoFactory.CreateImposto(imposto);
                _impostos.Add(newImposto);
            }
        }

        internal string GetIcmsCst()
        {
            var icms = _impostos.First(i => i is IcmsBase);
            return ((IcmsBase) icms).Cst;
        }

        public string GetPisCst()
        {
            var pis = _impostos.First(i => i is PisBase);
            return ((PisBase)pis).Cst;
        }
    }
}
