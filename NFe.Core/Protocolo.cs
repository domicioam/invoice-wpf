using System;
using System.Text.RegularExpressions;
using NFe.Core.Sefaz;
using NFe.Core.XmlSchemas.NfeConsulta2.Retorno;

namespace NFe.Core.NotasFiscais.Sefaz.NfeConsulta2
{
    public class Protocolo
    {
        public TProtNFe protNFe { get; }

        public Protocolo(TProtNFe protNFe)
        {
            this.protNFe = protNFe;
            protNFe.infProt.Id = null;
        }

        public DateTime DataRecebimento { get { return protNFe.infProt.dhRecbto; } }

        public string Numero
        {
            get
            {
                return protNFe.infProt.nProt;
            }
        }

        public string Xml
        {
            get
            {
                var protSerialized = XmlUtil.Serialize(protNFe, string.Empty)
                                        .Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                                        .Replace("TProtNFe", "protNFe");

                return Regex.Replace(protSerialized, "<infProt (.*?)>", "<infProt>");
            }
        }
    }
}