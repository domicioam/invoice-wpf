using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.NotasFiscais;
using static NFe.Core.Icms;

namespace NFe.Core.Domain
{
    /*
     * Domain model
     */
    public class Impostos : IEnumerable<IImposto>
    {
        private readonly IList<IImposto> _impostos;

        public Impostos()
        {
            _impostos = new List<IImposto>();
        }

        public Impostos(IEnumerable<Imposto> impostos) : this()
        {
            var impostoFactory = new NotasFiscal.Impostos.ImpostoFactory();

            foreach (var imposto in impostos)
            {
                var newImposto = impostoFactory.CreateImposto(imposto);
                _impostos.Add(newImposto);
            }
        }

        internal CstEnum GetIcmsCst()
        {
            var icms = _impostos.First(i => i is Icms);
            return ((Icms) icms).Cst;
        }

        public string GetPisCst()
        {
            var pis = _impostos.First(i => i is Pis);
            return ((Pis)pis).Cst.Value.ToString();
        }

        public IEnumerator<IImposto> GetEnumerator()
        {
            return _impostos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _impostos.GetEnumerator();
        }
    }
}
