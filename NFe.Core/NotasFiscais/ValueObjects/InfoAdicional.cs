using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NFe.Core.Cadastro.Ibpt;
using NFe.Core.NotasFiscais.Entities;

namespace NFe.Core.NotasFiscais
{
    public class InfoAdicional
    {
        public InfoAdicional(List<Produto> produtos)
        {
            var ncmList = produtos.Select(p => p.Ncm).ToList();
            var ibptList = IbptManager.GetIbptByNcmList(ncmList);

            double impostoEstudalTotal = 0;
            double impostoFederalTotal = 0;

            foreach (var produto in produtos)
            {
                var ibpt = ibptList.FirstOrDefault(i => i.NCM == produto.Ncm);

                impostoEstudalTotal += produto.ValorTotal * ibpt.TributacaoEstadual / 100;
                impostoFederalTotal += produto.ValorTotal * ibpt.TributacaoFederal / 100;
            }

            var culture = new CultureInfo("pt-BR");

            InfoAdicionalComplementar = string.Format(
                "Valor Aproximado dos tributos: R${0}. Federais: R${1}, Estaduais: R${2} Fonte: IBPT",
                string.Format(culture, "{0:N2}", impostoEstudalTotal + impostoFederalTotal),
                string.Format(culture, "{0:N2}", impostoFederalTotal),
                string.Format(culture, "{0:N2}", impostoEstudalTotal));
        }

        public string InfoAdicionalFisco { get; set; }
        public string InfoAdicionalComplementar { get; set; }
    }
}