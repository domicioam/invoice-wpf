using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;
using static NFe.Core.Icms;

namespace NFe.Core.NotasFiscais.Entities
{
    /*
     * Domain model
     */
    public class Impostos : IEnumerable<NotasFiscais.Imposto>
    {
        private readonly IList<NotasFiscais.Imposto> _impostos;

        public Impostos()
        {
            _impostos = new List<NotasFiscais.Imposto>();
        }

        public Impostos(IEnumerable<Imposto> impostos, decimal valorProduto)
        {
            var impostoFactory = new ImpostoFactory();

            foreach (var imposto in impostos)
            {
                var newImposto = impostoFactory.CreateImposto(imposto, valorProduto);
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
            return ((Pis)pis).Cst.Id.ToString();
        }

        public IEnumerator<NotasFiscais.Imposto> GetEnumerator()
        {
            return _impostos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _impostos.GetEnumerator();
        }
    }
}
