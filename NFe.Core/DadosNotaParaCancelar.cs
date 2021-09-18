using NFe.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Sefaz.Facades
{
    public class DadosNotaParaCancelar
    {
        public DadosNotaParaCancelar(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, string chaveNFe, string protocoloAutorizacao, Modelo modeloNota)
        {
            this.ufEmitente = ufEmitente;
            this.codigoUf = codigoUf;
            this.cnpjEmitente = cnpjEmitente;
            this.chaveNFe = chaveNFe;
            this.protocoloAutorizacao = protocoloAutorizacao;
            this.modeloNota = modeloNota;
        }

        public string ufEmitente { get; set; }
        public CodigoUfIbge codigoUf { get; set; }
        public string cnpjEmitente { get; set; }
        public string chaveNFe { get; set; }
        public string protocoloAutorizacao { get; set; }
        public Modelo modeloNota { get; set; }
    }
}
