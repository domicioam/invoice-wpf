using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Entitities.Enums;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.Utils.Conversores.Enums.Autorizacao;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using ItemChoiceType3 = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType3;
using ItemChoiceType6 = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType6;
using ItemsChoiceType5 = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.ItemsChoiceType5;
using TAmb = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TAmb;
using TEndereco = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TEndereco;
using TEnderEmi = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TEnderEmi;
using TEnderEmiCPais = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TEnderEmiCPais;
using TEnderEmiXPais = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TEnderEmiXPais;
using TFinNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TFinNFe;
using TMod = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TMod;
using TNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFe;
using TNFeInfNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFe;
using TNFeInfNFeDest = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDest;
using TNFeInfNFeDestIndIEDest = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDestIndIEDest;
using TNFeInfNFeDet = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDet;
using TNFeInfNFeDetImposto = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImposto;
using TNFeInfNFeDetImpostoCOFINS = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoCOFINS;
using TNFeInfNFeDetImpostoCOFINSCOFINSAliq =
    NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoCOFINSCOFINSAliq;
using TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST =
    NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST;
using TNFeInfNFeDetImpostoICMS = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoICMS;
using TNFeInfNFeDetImpostoICMSICMS40 = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoICMSICMS40;
using TNFeInfNFeDetImpostoICMSICMS40CST = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoICMSICMS40CST;
using TNFeInfNFeDetImpostoICMSICMS60 = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoICMSICMS60;
using TNFeInfNFeDetImpostoICMSICMS60CST = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoICMSICMS60CST;
using TNFeInfNFeDetImpostoPIS = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoPIS;
using TNFeInfNFeDetImpostoPISPISNT = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoPISPISNT;
using TNFeInfNFeDetImpostoPISPISNTCST = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImpostoPISPISNTCST;
using TNFeInfNFeDetProd = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetProd;
using TNFeInfNFeDetProdComb = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetProdComb;
using TNFeInfNFeDetProdIndTot = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetProdIndTot;
using TNFeInfNFeEmit = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeEmit;
using TNFeInfNFeEmitCRT = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeEmitCRT;
using TNFeInfNFeIde = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIde;
using TNFeInfNFeIdeIdDest = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeIdDest;
using TNFeInfNFeIdeIndFinal = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeIndFinal;
using TNFeInfNFeIdeIndPres = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeIndPres;
using TNFeInfNFeIdeTpEmis = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeTpEmis;
using TNFeInfNFeIdeTpImp = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeTpImp;
using TNFeInfNFeIdeTpNF = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeTpNF;
using TNFeInfNFeInfAdic = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeInfAdic;
using TNFeInfNFePag = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePag;
using TNFeInfNFePagDetPag = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePagDetPag;
using TNFeInfNFePagDetPagTPag = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePagDetPagTPag;
using TNFeInfNFeSupl = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeSupl;
using TNFeInfNFeTotal = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotal;
using TNFeInfNFeTotalICMSTot = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalICMSTot;
using TNFeInfNFeTransp = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTransp;
using TNFeInfNFeTranspModFrete = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTranspModFrete;
using TNFeInfNFeTranspTransporta = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTranspTransporta;
using Torig = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.Torig;
using TProcEmi = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TProcEmi;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;
using TVeiculo = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TVeiculo;

/** O método para pegar a nota fiscal a partir do xml encontra-se em NFeAutorizacaoNormal.GetNotaFiscalFromNfeProcXML() **/

namespace NFe.Core.Sefaz
{
    public static class XmlUtil
    {
        public static string Serialize<T>(T value, string namespaceName)
        {
            if (value == null) throw new ArgumentNullException();

            var xmlSerializer = new XmlSerializer(typeof(T));
            var xsn = new XmlSerializerNamespaces();
            xsn.Add("", namespaceName);

            using (var stream = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(stream, new UTF8Encoding(false)))
                {
                    xmlSerializer.Serialize(writer, value, xsn);
                    var streamToRead = (MemoryStream)writer.BaseStream;
                    var encoding = new UTF8Encoding();
                    return encoding.GetString(streamToRead.ToArray());
                }
            }
        }

        public static object Deserialize<T>(string xml)
        {
            if (xml == null) throw new ArgumentNullException();

            var xmlSerializer = new XmlSerializer(typeof(T));

            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(xml);

            using (var stream = new MemoryStream(byteArray))
            {
                return xmlSerializer.Deserialize(stream);
            }
        }

        internal static void SalvarXmlNFeComErro(NotaFiscal notaFiscal, XmlNode node)
        {
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Notas Fiscais");
            var notasComErroDir = Path.Combine(appDataDir, "Notas com erro");

            if (!Directory.Exists(notasComErroDir)) Directory.CreateDirectory(notasComErroDir);

            using (var stream =
                File.Create(Path.Combine(notasComErroDir, notaFiscal.Identificacao.Chave + " - erro.xml")))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(node.OuterXml);
                }
            }
        }

        internal static string GerarNfeProcXml(TNFe nfe, QrCode urlQrCode, TProtNFe protocolo = null)
        {
            var nfeProc = new TNfeProc();
            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
            var nfeSerializada = Serialize(nfe, nFeNamespaceName);
            nfeProc.NFe =
                (XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TNFe)
                Deserialize<XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TNFe>(nfeSerializada);

            if (nfeProc.NFe.infNFeSupl != null) nfeProc.NFe.infNFeSupl.qrCode = "";

            if (protocolo != null)
            {
                var protocoloSerializado = Serialize(protocolo, nFeNamespaceName);
                nfeProc.protNFe =
                    (XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TProtNFe)
                    Deserialize<XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TProtNFe>(protocoloSerializado);
            }
            else
            {
                nfeProc.protNFe = new XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TProtNFe();
            }

            nfeProc.versao = "4.00";
            var result = Serialize(nfeProc, nFeNamespaceName).Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            if (nfeProc.NFe.infNFeSupl != null)
                result = result.Replace("<qrCode />", "<qrCode>" + urlQrCode + "</qrCode>")
                    .Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            else
                result = result.Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");

            return result;
        }

        internal static string GerarXmlLoteNFe(NotaFiscal notaFiscal, string nFeNamespaceName)
        {
            var ide = ModelToSefazAdapter.GetIdentificacao(notaFiscal);
            var emit = ModelToSefazAdapter.GetEmitente(notaFiscal);
            var det = ModelToSefazAdapter.GetDetalhamentoProdutos(notaFiscal);
            var pag = ModelToSefazAdapter.GetPagamento(notaFiscal);
            var transp = ModelToSefazAdapter.GetTransporte(notaFiscal);
            var infAdic = ModelToSefazAdapter.GetInformacaoAdicional(notaFiscal);
            var total = ModelToSefazAdapter.GetTotal(notaFiscal);

            var infNFe = new TNFeInfNFe();

            if (notaFiscal.Destinatario != null)
            {
                var dest = ModelToSefazAdapter.GetDestinatario(notaFiscal);
                infNFe.dest = dest;
            }

            infNFe.ide = ide;
            infNFe.emit = emit;
            infNFe.det = det;
            infNFe.pag = pag;
            infNFe.transp = transp;
            infNFe.infAdic = infAdic;
            infNFe.total = total;
            infNFe.versao = notaFiscal.VersaoLayout;
            infNFe.Id = "NFe" + notaFiscal.Identificacao.Chave;

            var nfe = new TNFe();
            nfe.infNFe = infNFe;

            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                nfe.infNFeSupl = new TNFeInfNFeSupl
                { qrCode = "", urlChave = "http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx" };
            else
                nfe.infNFeSupl = null;

            var nfeArray = new TNFe[1];
            nfeArray[0] = nfe;

            var lote = new TEnviNFe();
            lote.idLote = "999999"; //qual a regra pra gerar o id?
            lote.indSinc = TEnviNFeIndSinc.Item1; //apenas uma nota no lote
            lote.versao = "4.00";
            lote.NFe = nfeArray;

            var parametroXml = Serialize(lote, nFeNamespaceName);
            parametroXml = parametroXml.Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");

            return parametroXml;
        }

        internal static string GerarXmlListaNFe(List<string> notasFiscais)
        {
            var notasConcatenadas = new StringBuilder();

            for (var i = 0; i < notasFiscais.Count; i++)
            {
                var nfeProc = new XmlDocument();
                nfeProc.LoadXml(notasFiscais[i]);
                notasConcatenadas.Append(nfeProc.GetElementsByTagName("NFe")[0].OuterXml);
            }

            return notasConcatenadas.ToString();
        }


    }
}