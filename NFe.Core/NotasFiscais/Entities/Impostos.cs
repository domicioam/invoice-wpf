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
        private IEnumerable<Imposto> _impostos;
        private string _cfop;

        public Impostos()
        {
            _impostos = new List<Imposto>();
        }

        public Impostos(IEnumerable<Imposto> impostos, string cfop)
        {
            this._impostos = impostos;
            _cfop = cfop;
        }

        internal string GetIcmsCst()
        {
            return _impostos.First(i => i.TipoImposto == TipoImposto.Icms).CST;
        }

        public string GetPisCst()
        {
            return _impostos.First(i => i.TipoImposto == TipoImposto.PIS).CST;
        }
    }
}
