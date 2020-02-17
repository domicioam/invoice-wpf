using System.Collections.Generic;
using System.Linq;
using NFe.Core.NotasFiscais;

namespace NFe.Core.Cadastro.Imposto
{
    public static class IbptManager
    {
        private static readonly List<Ibpt> _ibptList;

        static IbptManager()
        {
            _ibptList = new List<Ibpt>();

            _ibptList.Add(new Ibpt("27111910", "Gas liquefeito de petroleo(glp)", 13.45, 12.0));
            _ibptList.Add(new Ibpt("22021000", "Agua incl.mineral/gaseif.adicion.acucar, aromatizada,etc", 15.4, 31.0));
            _ibptList.Add(new Ibpt("84811000", "Valvulas redutoras de pressao", 13.45, 12.0));
            _ibptList.Add(new Ibpt("73110000", "Recipientes de ferro/aco,p/gases comprimidos/ liquefeit.", 6.77, 12.0));
        }

        public static List<Ibpt> GetIbptByNcmList(List<string> ncmList)
        {
            return _ibptList.Where(i => ncmList.Contains(i.NCM)).ToList();
        }
    }
}