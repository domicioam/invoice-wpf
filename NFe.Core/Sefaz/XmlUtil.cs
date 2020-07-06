using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NFe.Core.Entitities;
using NFe.Core.Entitities.Enums;
using NFe.Core.NotasFiscais;
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
            var ide = GetIdentificacao(notaFiscal);
            var emit = GetEmitente(notaFiscal);
            var det = GetDetalhamentoProdutos(notaFiscal);
            var pag = GetPagamento(notaFiscal);
            var transp = GetTransporte(notaFiscal);
            var infAdic = GetInformacaoAdicional(notaFiscal);
            var total = GetTotal(notaFiscal);

            var infNFe = new TNFeInfNFe();

            if (notaFiscal.Destinatario != null)
            {
                var dest = GetDestinatario(notaFiscal);
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

        internal static TNFeInfNFeTotal GetTotal(NotaFiscal notaFiscal)
        {
            var totalNfe = notaFiscal.TotalNFe;
            var total = new TNFeInfNFeTotal();
            total.ICMSTot = new TNFeInfNFeTotalICMSTot();
            total.ICMSTot.vBC = totalNfe.IcmsTotal.BaseCalculo.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vICMS = totalNfe.IcmsTotal.ValorTotalIcms.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vICMSDeson =
                totalNfe.IcmsTotal.ValorTotalDesonerado.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vFCP = "0.00";
            total.ICMSTot.vBCST = totalNfe.IcmsTotal.BaseCalculoST.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vST = totalNfe.IcmsTotal.ValorTotalST.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vFCPST = "0.00";
            total.ICMSTot.vFCPSTRet = "0.00";
            total.ICMSTot.vProd = totalNfe.IcmsTotal.ValorTotalProdutos.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vFrete = totalNfe.IcmsTotal.ValorTotalFrete.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vSeg = totalNfe.IcmsTotal.ValorTotalSeguro.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vDesc = totalNfe.IcmsTotal.ValorTotalDesconto.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vII = totalNfe.IcmsTotal.ValorTotalII.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vIPI = totalNfe.IcmsTotal.ValorTotalIpi.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vIPIDevol = "0.00";
            total.ICMSTot.vPIS = totalNfe.IcmsTotal.ValorTotalPis.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vCOFINS = totalNfe.IcmsTotal.ValorTotalCofins.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vOutro =
                totalNfe.IcmsTotal.ValorDespesasAcessorias.ToString("F", CultureInfo.InvariantCulture);
            total.ICMSTot.vNF = totalNfe.IcmsTotal.ValorTotalNFe.ToString("F", CultureInfo.InvariantCulture);

            return total;
        }

        internal static TNFeInfNFeInfAdic GetInformacaoAdicional(NotaFiscal notaFiscal)
        {
            var infAdic = new TNFeInfNFeInfAdic();
            infAdic.infCpl = notaFiscal.InfoAdicional.InfoAdicionalComplementar;
            infAdic.infAdFisco = notaFiscal.InfoAdicional.InfoAdicionalFisco;

            return infAdic;
        }

        internal static TNFeInfNFeTransp GetTransporte(NotaFiscal notaFiscal)
        {
            var transp = new TNFeInfNFeTransp();
            transp.modFrete = (TNFeInfNFeTranspModFrete)(int)notaFiscal.Transporte.ModalidadeFrete;

            if (notaFiscal.Transporte.Transportadora != null)
            {
                var transportadora = notaFiscal.Transporte.Transportadora;

                transp.transporta = new TNFeInfNFeTranspTransporta();
                transp.transporta.Item = transportadora.CpfCnpj;
                transp.transporta.ItemElementName =
                    transportadora.CpfCnpj.Length == 14 ? ItemChoiceType6.CNPJ : ItemChoiceType6.CPF;
                transp.transporta.xNome = transportadora.Nome;
                transp.transporta.IE = transportadora.InscricaoEstadual;
                transp.transporta.xEnder = transportadora.EnderecoCompleto;
                transp.transporta.xMun = transportadora.Municipio;
                transp.transporta.UF = TUfConversor.ToTUf(transportadora.SiglaUF);
                transp.transporta.UFSpecified = true;

                if (notaFiscal.Transporte.Veiculo != null)
                {
                    var veiculo = new TVeiculo
                    {
                        placa = notaFiscal.Transporte.Veiculo.Placa,
                        UF = TUfConversor.ToTUf(notaFiscal.Transporte.Veiculo.SiglaUF)
                    };
                    transp.Items = new object[] { veiculo };
                    transp.ItemsElementName = new[] { ItemsChoiceType5.veicTransp };
                }
            }

            return transp;
        }

        internal static TNFeInfNFePag GetPagamento(NotaFiscal notaFiscal)
        {
            if (notaFiscal.Pagamentos == null) return null;

            var listaPagamentos = new List<TNFeInfNFePagDetPag>();

            foreach (var pagamento in notaFiscal.Pagamentos)
            {
                var newPag = new TNFeInfNFePagDetPag
                {
                    vPag = pagamento.Valor.ToString("F", CultureInfo.InvariantCulture),
                    tPag = (TNFeInfNFePagDetPagTPag)(int)pagamento.FormaPagamento
                };

                listaPagamentos.Add(newPag);
            }

            var pag = new TNFeInfNFePag { detPag = listaPagamentos.ToArray() };

            return pag;
        }

        internal static TNFeInfNFeIde GetIdentificacao(NotaFiscal notaFiscal)
        {
            var ide = new TNFeInfNFeIde();
            ide.cUF = TCodUfIBGEConversor.ToTCodUfIBGE(notaFiscal.Identificacao.UF);
            ide.nNF = notaFiscal.Identificacao.Numero;
            ide.cNF = notaFiscal.Identificacao.Codigo;
            ide.natOp = notaFiscal.Identificacao.NaturezaOperacao;
            ide.mod = (TMod)(int)notaFiscal.Identificacao.Modelo;
            ide.serie = notaFiscal.Identificacao.Serie.ToString();
            ide.dhEmi = notaFiscal.Identificacao.DataHoraEmissao.ToString("yyyy-MM-ddTHH:mm:sszzz");
            ide.tpNF = (TNFeInfNFeIdeTpNF)(int)notaFiscal.Identificacao.TipoOperacao;
            ide.idDest = (TNFeInfNFeIdeIdDest)(int)notaFiscal.Identificacao.OperacaoDestino;
            ide.cMunFG = notaFiscal.Identificacao.CodigoMunicipio;
            ide.tpImp = (TNFeInfNFeIdeTpImp)(int)notaFiscal.Identificacao.FormatoImpressao;
            ide.tpEmis = (TNFeInfNFeIdeTpEmis)(int)notaFiscal.Identificacao.TipoEmissao;
            ide.tpAmb = (TAmb)(int)notaFiscal.Identificacao.Ambiente;
            ide.finNFe = (TFinNFe)(int)notaFiscal.Identificacao.FinalidadeEmissao;
            ide.indFinal = (TNFeInfNFeIdeIndFinal)(int)notaFiscal.Identificacao.FinalidadeConsumidor;
            ide.indPres = (TNFeInfNFeIdeIndPres)(int)notaFiscal.Identificacao.PresencaComprador;
            ide.procEmi = (TProcEmi)(int)notaFiscal.Identificacao.ProcessoEmissao;
            ide.verProc = notaFiscal.Identificacao.VersaoAplicativo;
            ide.cDV = notaFiscal.Identificacao.DigitoVerificador.ToString();

            if (notaFiscal.Identificacao.TipoEmissao == TipoEmissao.ContigenciaNfce ||
                notaFiscal.Identificacao.TipoEmissao == TipoEmissao.FsDa)
            {
                ide.dhCont = notaFiscal.Identificacao.DataHoraEntradaContigencia.ToString("yyyy-MM-ddTHH:mm:sszzz");
                ide.xJust = notaFiscal.Identificacao.JustificativaContigencia;
            }

            return ide;
        }

        internal static TNFeInfNFeEmit GetEmitente(NotaFiscal notaFiscal)
        {
            var emit = new TNFeInfNFeEmit();
            emit.Item = notaFiscal.Emitente.CNPJ;
            emit.xNome = notaFiscal.Emitente.Nome;
            emit.xFant = notaFiscal.Emitente.NomeFantasia;
            emit.IE = notaFiscal.Emitente.InscricaoEstadual;
            emit.IM = notaFiscal.Emitente.InscricaoMunicipal;
            emit.CNAE = notaFiscal.Emitente.CNAE;
            emit.CRT = (TNFeInfNFeEmitCRT)(int)notaFiscal.Emitente.CRT;
            emit.enderEmit = new TEnderEmi();
            emit.enderEmit.xLgr = notaFiscal.Emitente.Endereco.Logradouro;
            emit.enderEmit.nro = notaFiscal.Emitente.Endereco.Numero;
            emit.enderEmit.xBairro = notaFiscal.Emitente.Endereco.Bairro;
            emit.enderEmit.cMun = notaFiscal.Emitente.Endereco.CodigoMunicipio;
            emit.enderEmit.xMun = notaFiscal.Emitente.Endereco.Municipio;
            emit.enderEmit.UF = TUfEmiConversor.TUfEmi(notaFiscal.Emitente.Endereco.UF);
            emit.enderEmit.CEP = notaFiscal.Emitente.Endereco.Cep;
            emit.enderEmit.cPais = TEnderEmiCPais.Item1058;
            emit.enderEmit.xPais = TEnderEmiXPais.Brasil;
            emit.enderEmit.fone = notaFiscal.Emitente.Telefone;

            return emit;
        }

        internal static TNFeInfNFeDest GetDestinatario(NotaFiscal notaFiscal)
        {
            var dest = new TNFeInfNFeDest();
            dest.Item = notaFiscal.Destinatario.Documento;

            switch (notaFiscal.Destinatario.TipoDestinatario)
            {
                case TipoDestinatario.PessoaFisica:
                    dest.ItemElementName = ItemChoiceType3.CPF;
                    break;
                case TipoDestinatario.PessoaJuridica:
                    dest.ItemElementName = ItemChoiceType3.CNPJ;
                    break;
                case TipoDestinatario.Estrangeiro:
                    dest.ItemElementName = ItemChoiceType3.idEstrangeiro;
                    break;
            }

            dest.xNome = notaFiscal.Destinatario.NomeRazao;

            if (notaFiscal.Destinatario.IsIsentoICMS)
                dest.indIEDest = TNFeInfNFeDestIndIEDest.Item2;
            else if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                dest.indIEDest = TNFeInfNFeDestIndIEDest.Item9;
            else
                dest.indIEDest = TNFeInfNFeDestIndIEDest.Item1;

            dest.IE = notaFiscal.Destinatario.InscricaoEstadual;
            dest.email = notaFiscal.Destinatario.Email;

            if (notaFiscal.Destinatario.Endereco != null)
            {
                dest.enderDest = new TEndereco();

                dest.enderDest.xLgr = notaFiscal.Destinatario.Endereco.Logradouro;
                dest.enderDest.nro = notaFiscal.Destinatario.Endereco.Numero;
                dest.enderDest.xBairro = notaFiscal.Destinatario.Endereco.Bairro;
                dest.enderDest.cMun = notaFiscal.Destinatario.Endereco.CodigoMunicipio;
                dest.enderDest.xMun = notaFiscal.Destinatario.Endereco.Municipio;
                dest.enderDest.UF = TUfDestConversor.TUf(notaFiscal.Destinatario.Endereco.UF);
                dest.enderDest.CEP = notaFiscal.Destinatario.Endereco.Cep;
                dest.enderDest.cPais = "1058";
                dest.enderDest.xPais = "Brasil";
                dest.enderDest.fone = notaFiscal.Destinatario.Telefone;
            }

            return dest;
        }

        internal static TNFeInfNFeDet[] GetDetalhamentoProdutos(NotaFiscal notaFiscal)
        {
            var detList = new List<TNFeInfNFeDet>();

            for (var i = 0; i < notaFiscal.Produtos.Count; i++)
            {
                var newDet = new TNFeInfNFeDet();
                newDet.prod = new TNFeInfNFeDetProd();
                newDet.prod.cProd = notaFiscal.Produtos[i].Codigo;
                newDet.prod.cEAN = "SEM GTIN";
                newDet.prod.xProd = notaFiscal.Produtos[i].Descricao;
                newDet.prod.NCM = notaFiscal.Produtos[i].Ncm;
                newDet.prod.CEST = notaFiscal.Produtos[i].Cest; //Não usado em produção
                newDet.prod.uCom = notaFiscal.Produtos[i].UnidadeComercial;
                newDet.prod.qCom = notaFiscal.Produtos[i].QtdeUnidadeComercial.ToString();
                newDet.prod.vUnCom = notaFiscal.Produtos[i].ValorUnidadeComercial
                    .ToString("F", CultureInfo.InvariantCulture);
                newDet.prod.vProd = notaFiscal.Produtos[i].ValorTotal.ToString("F", CultureInfo.InvariantCulture);
                newDet.prod.cEANTrib = "SEM GTIN";
                newDet.prod.uTrib = notaFiscal.Produtos[i].UnidadeComercial;
                newDet.prod.qTrib = notaFiscal.Produtos[i].QtdeUnidadeComercial.ToString();
                newDet.prod.vUnTrib = notaFiscal.Produtos[i].ValorUnidadeComercial
                    .ToString("F", CultureInfo.InvariantCulture);
                newDet.prod.CFOP = notaFiscal.Produtos[i].Cfop.ToString().Replace("Item", string.Empty);
                newDet.prod.indTot = TNFeInfNFeDetProdIndTot.Item1;

                //tratamento de produtos específico (combustíveis)

                if (notaFiscal.Identificacao.Modelo != Modelo.Modelo65 && notaFiscal.Produtos[i].Ncm.Equals("27111910"))
                {
                    var comb = new TNFeInfNFeDetProdComb();
                    comb.cProdANP = "210203001";
                    comb.UFCons = TUfConversor.ToTUf(notaFiscal.Destinatario.Endereco.UF);
                    comb.descANP = "GLP";
                    comb.pGLP = "100.00";
                    comb.vPart =
                        (notaFiscal.Produtos[i].ValorUnidadeComercial / 13).ToString("F", CultureInfo.InvariantCulture);

                    newDet.prod.uTrib = "KG";
                    newDet.prod.Items = new object[] { comb };
                }

                newDet.imposto = GetImposto(notaFiscal.Produtos[i]);
                newDet.nItem = (i + 1).ToString();

                detList.Add(newDet);
            }

            return detList.ToArray();
        }

        internal static TNFeInfNFeDetImposto GetImposto(Produto produto)
        {
            var imposto = new TNFeInfNFeDetImposto { Items = new object[1] };

            switch (produto.GrupoImpostos.Impostos.First(i => i.TipoImposto == TipoImposto.Icms).CST)
            {
                case TabelaIcmsCst.IcmsCobradoAnteriormentePorST:
                    var icms = new TNFeInfNFeDetImpostoICMS();
                    var icms60 = new TNFeInfNFeDetImpostoICMSICMS60();
                    icms60.orig = Torig.Item0;
                    icms60.CST = TNFeInfNFeDetImpostoICMSICMS60CST.Item60;
                    icms.Item = icms60;
                    imposto.Items[0] = icms;
                    break;

                case TabelaIcmsCst.NaoTributada:
                    var detIcms41 = new TNFeInfNFeDetImpostoICMS();
                    var icms41 = new TNFeInfNFeDetImpostoICMSICMS40();
                    icms41.orig = Torig.Item0;
                    icms41.CST = TNFeInfNFeDetImpostoICMSICMS40CST.Item41;
                    detIcms41.Item = icms41;
                    imposto.Items[0] = detIcms41;
                    break;

                default:
                    throw new ArgumentException();
            }

            imposto.PIS = new TNFeInfNFeDetImpostoPIS();
            var pisNt = new TNFeInfNFeDetImpostoPISPISNT();

            switch (produto.GrupoImpostos.Impostos.First(i => i.TipoImposto == TipoImposto.PIS).CST)
            {
                case TabelaPisCst.OperacaoTributavelMonofasicaRevendaAliquotaZero:
                    pisNt.CST = TNFeInfNFeDetImpostoPISPISNTCST.Item04;
                    imposto.PIS.Item = pisNt;
                    break;

                case TabelaPisCst.OperacaoTributavelPorST:
                    pisNt.CST = TNFeInfNFeDetImpostoPISPISNTCST.Item05;
                    imposto.PIS.Item = pisNt;
                    break;

                case TabelaPisCst.OperacaoTributavelAliquotaZero:
                    pisNt.CST = TNFeInfNFeDetImpostoPISPISNTCST.Item06;
                    imposto.PIS.Item = pisNt;
                    break;

                case TabelaPisCst.OperacaoIsentaContribuicao:
                    pisNt.CST = TNFeInfNFeDetImpostoPISPISNTCST.Item07;
                    imposto.PIS.Item = pisNt;
                    break;

                case TabelaPisCst.OperacaoSemIncidenciaContribuicao:
                    pisNt.CST = TNFeInfNFeDetImpostoPISPISNTCST.Item08;
                    imposto.PIS.Item = pisNt;
                    break;

                case TabelaPisCst.OperacaoComSuspensaoContribuicao:
                    pisNt.CST = TNFeInfNFeDetImpostoPISPISNTCST.Item09;
                    imposto.PIS.Item = pisNt;
                    break;
            }

            //Deixar dinâmico depois que alterar interface
            imposto.COFINS = new TNFeInfNFeDetImpostoCOFINS();

            var cofins = new TNFeInfNFeDetImpostoCOFINSCOFINSAliq
            {
                CST = TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item01,
                vBC = "0.00",
                pCOFINS = "0.0000",
                vCOFINS = "0.00"
            };

            imposto.COFINS.Item = cofins;

            return imposto;
        }
    }
}