using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.Extensions;
using NFe.Core.NotaFiscal;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core
{
    public abstract class CofinsBase : NFe.Core.NotaFiscal.Interface.Imposto
    {
        protected CofinsBase(CstEnum cst)
        {
            Cst = cst;
        }

        public CstEnum Cst { get; }
        public virtual decimal Valor { get; }

        public class CstEnum : Enumeration
        {
            public CstEnum(int id, string name) : base(id, name)
            {
            }

            public static readonly CstEnum CST01 = new CstEnum(01, "CST01");
            public static readonly CstEnum CST02 = new CstEnum(02, "CST02");

            public static implicit operator TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST(CstEnum cst)
            {
                if (cst == CST01)
                    return TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item01;
                else if (cst == CST02)
                    return TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item02;
                else
                    throw new InvalidOperationException($"CST não suportado para Cofins.");
            }
        }
    }
}