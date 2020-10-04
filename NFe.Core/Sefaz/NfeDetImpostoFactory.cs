using System;
using System.Collections.Generic;
using System.Linq;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core.Sefaz
{
    internal class NfeDetImpostoFactory
    {
        private readonly ImpostoCreatorFactory _impostoCreatorFactory;

        public NfeDetImpostoFactory(ImpostoCreatorFactory directorFactory)
        {
            _impostoCreatorFactory = directorFactory;
        }

        internal TNFeInfNFeDetImposto CreateNfeDetImposto(Impostos impostos)
        {
            var nfeDetImposto = new TNFeInfNFeDetImposto();

            nfeDetImposto.Items = FillImpostoItems(impostos).ToArray();
            nfeDetImposto.PIS = FillImpostoPis(impostos);
            nfeDetImposto.PISST = FillImpostoPisst(impostos);
            nfeDetImposto.COFINS = FillImpostoCofins(impostos);
            nfeDetImposto.COFINSST = FillImpostoCofinsst(impostos);
            nfeDetImposto.ICMSUFDest = FillImpostoIcmsUfDestino(impostos);

            return nfeDetImposto;
        }

        private List<object> FillImpostoItems(Impostos impostos)
        {
            // Preenche Items
            var impostoItems = impostos.Where(imposto =>
                imposto is Icms && !(imposto is IcmsUfDestino) || imposto is II || imposto is Ipi || imposto is Issqn);

            var items = new List<object>();

            foreach (var impostoItem in impostoItems)
            {
                var creator = _impostoCreatorFactory.Create(impostoItem);
                var detImposto = creator.Create(impostoItem);

                items.Add(detImposto);
            }

            return items;
        }

        private TNFeInfNFeDetImpostoICMSUFDest FillImpostoIcmsUfDestino(Impostos impostos)
        {
            var icmsUfDestino = impostos.FirstOrDefault(i => i is IcmsUfDestino);

            if (icmsUfDestino == null) 
                return null;

            var director = _impostoCreatorFactory.Create(icmsUfDestino);
            var detImpostoIcmsufDest = (TNFeInfNFeDetImpostoICMSUFDest) director.Create(icmsUfDestino);

            return detImpostoIcmsufDest;
        }

        private TNFeInfNFeDetImpostoCOFINSST FillImpostoCofinsst(Impostos impostos)
        {
            var cofinsSt = impostos.FirstOrDefault(i => i is CofinsSt);

            if (cofinsSt == null) 
                return null;

            var director = _impostoCreatorFactory.Create(cofinsSt);
            var detImpostoCofinsst = (TNFeInfNFeDetImpostoCOFINSST) director.Create(cofinsSt);

            return detImpostoCofinsst;
        }

        private TNFeInfNFeDetImpostoCOFINS FillImpostoCofins(Impostos impostos)
        {
            var cofins = impostos.FirstOrDefault(i => i is Cofins);

            if (cofins == null) 
                return null;

            var director = _impostoCreatorFactory.Create(cofins);
            var detImpostoCofins = (TNFeInfNFeDetImpostoCOFINS) director.Create(cofins);

            return detImpostoCofins;
        }

        private TNFeInfNFeDetImpostoPISST FillImpostoPisst(Impostos impostos)
        {
            var pisSt = impostos.FirstOrDefault(i => i is PisSt);

            if (pisSt == null) 
                return null;

            var director = _impostoCreatorFactory.Create(pisSt);
            var detImpostoPisst = (TNFeInfNFeDetImpostoPISST) director.Create(pisSt);

            return detImpostoPisst;
        }

        private TNFeInfNFeDetImpostoPIS FillImpostoPis(Impostos impostos)
        {
            var pis = impostos.FirstOrDefault(i => i is Pis);

            if (pis == null) 
                return null;

            var director = _impostoCreatorFactory.Create(pis);
            var detImpostoPis = (TNFeInfNFeDetImpostoPIS) director.Create(pis);

            return detImpostoPis;
        }
    }
}