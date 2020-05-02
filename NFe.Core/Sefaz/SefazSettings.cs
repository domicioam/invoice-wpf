using NFe.Core.NotasFiscais;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Sefaz
{
    public class SefazSettings
    {
        public Ambiente Ambiente { get; set; }

        public SefazSettings()
        {
            var sefazEnvironment = ConfigurationManager.AppSettings["sefazEnvironment"];

            if(sefazEnvironment == "production")
            {
                Ambiente = Ambiente.Producao;
            }
            else
            {
                Ambiente = Ambiente.Homologacao;
            }

        }
    }
}
