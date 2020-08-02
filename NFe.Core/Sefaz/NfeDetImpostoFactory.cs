using System;
using System.Collections.Generic;
using System.Linq;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core.Sefaz
{
    internal class NfeDetImpostoFactory
    {
        private readonly ImpostoDirectorFactory _directorFactory;

        public NfeDetImpostoFactory(ImpostoDirectorFactory directorFactory)
        {
            _directorFactory = directorFactory;
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
                var director = _directorFactory.CreateDirector(impostoItem);
                var detImposto = director.FillInImpostoDetails(impostoItem);

                items.Add(detImposto);
            }

            return items;
        }

        private TNFeInfNFeDetImpostoICMSUFDest FillImpostoIcmsUfDestino(Impostos impostos)
        {
            var icmsUfDestino = impostos.FirstOrDefault(i => i is IcmsUfDestino);

            if (icmsUfDestino == null) 
                return null;

            var strategy = _directorFactory.CreateDirector(icmsUfDestino);
            var detImpostoIcmsufDest = (TNFeInfNFeDetImpostoICMSUFDest) strategy.FillInImpostoDetails(icmsUfDestino);

            return detImpostoIcmsufDest;
        }

        private TNFeInfNFeDetImpostoCOFINSST FillImpostoCofinsst(Impostos impostos)
        {
            var cofinsSt = impostos.FirstOrDefault(i => i is CofinsSt);

            if (cofinsSt == null) 
                return null;

            var strategy = _directorFactory.CreateDirector(cofinsSt);
            var detImpostoCofinsst = (TNFeInfNFeDetImpostoCOFINSST) strategy.FillInImpostoDetails(cofinsSt);

            return detImpostoCofinsst;
        }

        private TNFeInfNFeDetImpostoCOFINS FillImpostoCofins(Impostos impostos)
        {
            var cofins = impostos.FirstOrDefault(i => i is Cofins);

            if (cofins == null) 
                return null;

            var strategy = _directorFactory.CreateDirector(cofins);
            var detImpostoCofins = (TNFeInfNFeDetImpostoCOFINS) strategy.FillInImpostoDetails(cofins);

            return detImpostoCofins;
        }

        private TNFeInfNFeDetImpostoPISST FillImpostoPisst(Impostos impostos)
        {
            var pisSt = impostos.FirstOrDefault(i => i is PisSt);

            if (pisSt == null) 
                return null;

            var strategy = _directorFactory.CreateDirector(pisSt);
            var detImpostoPisst = (TNFeInfNFeDetImpostoPISST) strategy.FillInImpostoDetails(pisSt);

            return detImpostoPisst;
        }

        private TNFeInfNFeDetImpostoPIS FillImpostoPis(Impostos impostos)
        {
            var pis = impostos.FirstOrDefault(i => i is Pis);

            if (pis == null) 
                return null;

            var strategy = _directorFactory.CreateDirector(pis);
            var detImpostoPis = (TNFeInfNFeDetImpostoPIS) strategy.FillInImpostoDetails(pis);

            return detImpostoPis;
        }
    }
}