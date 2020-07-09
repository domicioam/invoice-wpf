using System;
using System.Linq;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;

namespace NFe.Core.NotasFiscais.Entities
{
    public class Produto
    {
        public Produto(Impostos impostos, int id, string cfop, string codigo, string descricao, string ncm,
            int qtdeUnidadeComercial, string unidadeComercial,
            double valorUnidadeComercial, double valorDesconto, bool isProducao)
        {
            Impostos = impostos;
            Id = id;
            Cfop = (TCfop) Enum.Parse(typeof(TCfop), "Item" + cfop);
            Codigo = codigo;
            Descricao = descricao;
            Ncm = ncm;
            QtdeUnidadeComercial = qtdeUnidadeComercial;
            UnidadeComercial = unidadeComercial;
            ValorUnidadeComercial = valorUnidadeComercial; //quando unidade comercial não for UN, isso aqui muda, arrumar no banco (ver exemplo no teste unitário NFeManager)
            ValorTotal = QtdeUnidadeComercial * ValorUnidadeComercial;
            ValorDesconto = valorDesconto;

            if (isProducao) Cest = "0104300";
        }

        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public string Ncm { get; set; }
        public string Cest { get; set; }
        public TCfop Cfop { get; set; }
        public string UnidadeComercial { get; set; }
        public Impostos Impostos { get; set; }
        public int QtdeUnidadeComercial { get; set; }
        public double ValorUnidadeComercial { get; set; }

        public double ValorTotal { get; set; }

        public double ValorDesconto { get; set; }

        public string CstTexto
        {
            get { return Impostos.GetIcmsCst(); }
        }

        public string CfopTexto
        {
            get { return Cfop.ToString().Replace("Item", string.Empty); }
        }
    }
}