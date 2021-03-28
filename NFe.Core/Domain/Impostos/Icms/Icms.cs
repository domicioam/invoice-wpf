using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.Extensions;
using NFe.Core.NotaFiscal;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core
{
    public abstract class Icms : NotaFiscal.Interface.Imposto
    {
        protected Icms(CstEnum cst, OrigemMercadoria origem, decimal aliquota)
        {
            Cst = cst;
            Origem = origem;
            Aliquota = aliquota;
        }

        public CstEnum Cst { get; }
        public OrigemMercadoria Origem { get; }
        public virtual decimal BaseCalculo { get; }

        public virtual decimal Valor { get; }
        public decimal Aliquota { get; }
        public class CstEnum : Enumeration
        {
            public CstEnum(int id, string name) : base(id, name)
            {
            }

            public static readonly CstEnum CST60 = new CstEnum(60, "CST60");
            public static readonly CstEnum CST41 = new CstEnum(41, "CST41");
            public static readonly CstEnum CST40 = new CstEnum(40, "CST40");
            public static readonly CstEnum CST10 = new CstEnum(10, "CST10");

            public static implicit operator TNFeInfNFeDetImpostoICMSICMS60CST(CstEnum cst)
            {
                if (cst == CST60)
                {
                    return TNFeInfNFeDetImpostoICMSICMS60CST.Item60;
                }
                else
                {
                    throw new InvalidOperationException($"CST não suportado para ICMS 60.");
                }
            }

            public static implicit operator TNFeInfNFeDetImpostoICMSICMS40CST(CstEnum cst)
            {
                if (cst == CST40)
                {
                    return TNFeInfNFeDetImpostoICMSICMS40CST.Item40;
                }
                else if(cst == CST41)
                {
                    return TNFeInfNFeDetImpostoICMSICMS40CST.Item41;
                }
                else
                {
                    throw new InvalidOperationException($"CST não suportado para ICMS 40.");
                }
            }
        }
    }
}
