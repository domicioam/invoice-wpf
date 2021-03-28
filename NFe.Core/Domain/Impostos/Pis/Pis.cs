using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.Extensions;
using NFe.Core.Domain;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core
{
    public abstract class Pis : NFe.Core.Domain.Interface.Imposto
    {
        protected Pis(CstEnum cst)
        {
            Cst = cst;
        }

        public CstEnum Cst { get; }
        public decimal Valor { get; set; }

        public class CstEnum : Enumeration
        {
            public CstEnum(int id, string name) : base(id, name)
            {
            }

            public static readonly CstEnum CST04 = new CstEnum(04, "CST04");
            public static readonly CstEnum CST05 = new CstEnum(05, "CST05");
            public static readonly CstEnum CST06 = new CstEnum(06, "CST06");
            public static readonly CstEnum CST07 = new CstEnum(07, "CST07");
            public static readonly CstEnum CST08 = new CstEnum(08, "CST08");
            public static readonly CstEnum CST09 = new CstEnum(09, "CST09");

            public static implicit operator TNFeInfNFeDetImpostoPISPISNTCST(CstEnum cst)
            {
                if (cst == CST04)
                    return TNFeInfNFeDetImpostoPISPISNTCST.Item04;
                else if (cst == CST05)
                    return TNFeInfNFeDetImpostoPISPISNTCST.Item05;
                else if (cst == CST06)
                    return TNFeInfNFeDetImpostoPISPISNTCST.Item06;
                else if (cst == CST07)
                    return TNFeInfNFeDetImpostoPISPISNTCST.Item07;
                else if (cst == CST08)
                    return TNFeInfNFeDetImpostoPISPISNTCST.Item08;
                else
                    throw new InvalidOperationException($"CST não suportado para Pis.");
            }
        }
    }
}