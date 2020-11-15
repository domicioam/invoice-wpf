using NFe.Core.Extensions;
using NFe.Core.NotasFiscais;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class OrigemMercadoria : Enumeration
    {
        public static readonly OrigemMercadoria Nacional = new OrigemMercadoria(0, "Nacional");
        public static readonly OrigemMercadoria EstrangeiraImportacaoDireta = new OrigemMercadoria(1, "EstrangeiraImportacaoDireta");
        public static readonly OrigemMercadoria EstrangeiraMercadoInterno = new OrigemMercadoria(2, "EstrangeiraMercadoInterno");

        public OrigemMercadoria(int id, string name) : base(id, name)
        {
        }

        public static implicit operator Torig(OrigemMercadoria origemMercadoria)
        {
            if(origemMercadoria == Nacional)
            {
                return Torig.Item0;
            } else if(origemMercadoria == EstrangeiraImportacaoDireta)
            {
                return Torig.Item1;
            } else if (origemMercadoria == EstrangeiraMercadoInterno)
            {
                return Torig.Item2;
            } else
            {
                throw new InvalidOperationException($"Origem de mercadoria não suportada: {origemMercadoria}.");
            }
        }
    }

    public class IcmsCobradoAnteriormentePorSubstituicaoTributaria : Icms
    {
        public IcmsCobradoAnteriormentePorSubstituicaoTributaria(decimal aliquota, decimal baseCalculo, FundoCombatePobreza fcp, OrigemMercadoria origem) : base(CstEnum.CST60, origem)
        {
            Aliquota = aliquota;
            BaseCalculo = baseCalculo;
            _fcp = fcp;
        }

        public decimal BaseCalculo { get;  }

        private FundoCombatePobreza _fcp;

        public decimal Valor { get { return BaseCalculo * (Aliquota / 100); }  }
        public decimal BaseCalculoFundoCombatePobreza { get { return _fcp.BaseCalculo; } }
        public decimal AliquotaFCP { get { return _fcp.Aliquota; }  }
        public decimal ValorFundoCombatePobreza { get { return _fcp.Valor; }  }
        public decimal Aliquota { get;  }
    }
}
 