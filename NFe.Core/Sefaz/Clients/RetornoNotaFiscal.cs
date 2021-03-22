using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using NFe.Core.Entitities;
using NFe.Core.Entitities.Enums;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using Retorno = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using TNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFe;
using TUf = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TUf;
using TUfEmi = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TUfEmi;

namespace NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao
{
   public class RetornoNotaFiscal
   {
      public string CodigoStatus { get; set; }
      public string Chave { get; set; }
      public string Motivo { get; set; }
      public string UrlQrCode { get; set; }
      public string Protocolo { get; set; }
      public string DataAutorizacao { get; set; }
      public string Xml { get; set; }
   }
}
