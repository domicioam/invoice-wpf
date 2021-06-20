using System;
using System.Linq;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;

namespace NFe.Core.Domain
{
    public class Produto
    {
        public Produto(Impostos impostos, int id, string cfop, string codigo, string descricao, string ncm,
            int qtdeUnidadeComercial, string unidadeComercial,
            double valorUnidadeComercial, double desconto, bool isProducao, double frete, double seguro, double outros)
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
            Desconto = desconto;
            Frete = frete;
            Seguro = seguro;
            Outros = outros;

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

        public double Desconto { get; set; }

        public string CstTexto
        {
            get { return Impostos.GetIcmsCst().ToString(); }
        }

        public string CfopTexto
        {
            get { return Cfop.ToString().Replace("Item", string.Empty); }
        }

        public double Frete { get; }
        public double Seguro { get; }
        public double Outros { get; }
    }
}