using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

using NFe.Core.Domain;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using TNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFe;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;
using NFe.Core.Extensions;
using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.Utils.Conversores.Enums.Autorizacao;
using System.Globalization;
using System.Linq;

/** O método para pegar a nota fiscal a partir do xml encontra-se em NFeAutorizacaoNormal.GetNotaFiscalFromNfeProcXML() **/

namespace NFe.Core.Sefaz
{
    public class XmlUtil
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

        public static string GerarXmlLoteNFe(NotaFiscal notaFiscal, string nFeNamespaceName)
        {
            TEnviNFe lote = GetLoteNFe(notaFiscal);

            var parametroXml = Serialize(lote, nFeNamespaceName);
            return parametroXml.Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
        }

        public static TEnviNFe GetLoteNFe(NotaFiscal notaFiscal)
        {
            var ide = GetIdentificacao(notaFiscal);
            var emit = GetEmitente(notaFiscal);
            var det = GetDetalhamentoProdutos(notaFiscal);
            var pag = GetPagamento(notaFiscal);
            var transp = GetTransporte(notaFiscal);
            var infAdic = GetInformacaoAdicional(notaFiscal);
            var total = GetTotal(notaFiscal);

            var infNFe = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFe();

            if (notaFiscal.Destinatario != null)
            {
                infNFe.dest = GetDestinatario(notaFiscal);
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

            var nfe = new TNFe { infNFe = infNFe };

            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
            {
                nfe.infNFeSupl = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeSupl { qrCode = "", urlChave = "http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx" };
            }
            else
            {
                nfe.infNFeSupl = null;
            }

            var nfeArray = new TNFe[1];
            nfeArray[0] = nfe;

            var lote = new TEnviNFe
            {
                idLote = "999999",
                indSinc = TEnviNFeIndSinc.Item1,
                versao = "4.00",
                NFe = nfeArray
            };

            //qual a regra pra gerar o id?
            //apenas uma nota no lote
            return lote;
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotal GetTotal(NotaFiscal notaFiscal)
        {
            return new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotal
            {
                ICMSTot = notaFiscal.TotalNFe.IcmsTotal == null ? null : ConvertIcmsTotal(notaFiscal),
                ISSQNtot = ConvertIssqn(notaFiscal),
                retTrib = ConvertTributosFederais(notaFiscal)
            };
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalRetTrib ConvertTributosFederais(NotaFiscal notaFiscal)
        {
            var retencaoTributosFederais = notaFiscal.TotalNFe.RetencaoTributosFederais;
            if (retencaoTributosFederais == null) return null;

            return new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalRetTrib
            {
                vRetPIS = retencaoTributosFederais.TotalRetidoPis.AsNumberFormattedString(),
                vRetCOFINS = retencaoTributosFederais.TotalRetidoCofins.AsNumberFormattedString(),
                vRetCSLL = retencaoTributosFederais.TotalRetidoCsll.AsNumberFormattedString(),
                vBCIRRF = retencaoTributosFederais.BaseCalculoIrrf.AsNumberFormattedString(),
                vIRRF = retencaoTributosFederais.TotalRetidoIrrf.AsNumberFormattedString(),
                vBCRetPrev = retencaoTributosFederais.BaseCalculoRetencaoPrevidenciaSocial.AsNumberFormattedString(),
                vRetPrev = retencaoTributosFederais.TotalRetencaoPrevidenciaSocial.AsNumberFormattedString()
            };
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalISSQNtot ConvertIssqn(NotaFiscal notaFiscal)
        {
            var issqnTotal = notaFiscal.TotalNFe.IssqnTotal;
            if (issqnTotal == null) return null;

            return new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalISSQNtot
            {
                vServ = issqnTotal.TotalServicos.AsNumberFormattedString(),
                vBC = issqnTotal.BaseCalculo.AsNumberFormattedString(),
                vISS = issqnTotal.TotalIss.AsNumberFormattedString(),
                vPIS = issqnTotal.Pis.AsNumberFormattedString(),
                vCOFINS = issqnTotal.Cofins.AsNumberFormattedString(),
                dCompet = issqnTotal.DataPrestacaoServico.ToString("yyyy-MM-dd"),
                vDeducao = issqnTotal.DeducaoBaseCalculo.AsNumberFormattedString(),
                vOutro = issqnTotal.Outro.AsNumberFormattedString(),
                vDescIncond = issqnTotal.DescontoIncondicionado.AsNumberFormattedString(),
                vDescCond = issqnTotal.DescontoCondicionado.AsNumberFormattedString(),
                vISSRet = issqnTotal.TotalRetencaoIss.AsNumberFormattedString(),
                cRegTrib = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalISSQNtotCRegTrib)(int)issqnTotal.RegimeEspecialTributacao
            };
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeInfAdic GetInformacaoAdicional(NotaFiscal notaFiscal)
        {
            return new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeInfAdic
            {
                infCpl = notaFiscal.InfoAdicional.InfoAdicionalComplementar,
                infAdFisco = notaFiscal.InfoAdicional.InfoAdicionalFisco
            };
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTransp GetTransporte(NotaFiscal notaFiscal)
        {
            var transp = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTransp
            {
                modFrete = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTranspModFrete)(int)notaFiscal.Transporte.ModalidadeFrete
            };

            if (notaFiscal.Transporte.Transportadora == null)
                return transp;

            var transportadora = notaFiscal.Transporte.Transportadora;

            transp.transporta = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTranspTransporta
            {
                Item = transportadora.CpfCnpj,
                ItemElementName = transportadora.CpfCnpj.Length == 14 ? XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType6.CNPJ : XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType6.CPF,
                xNome = transportadora.Nome,
                IE = transportadora.InscricaoEstadual,
                xEnder = transportadora.EnderecoCompleto,
                xMun = transportadora.Municipio,
                UF = TUfConversor.ToTUf(transportadora.SiglaUF),
                UFSpecified = true
            };

            if (notaFiscal.Transporte.Veiculo == null)
                return transp;

            var veiculo = new XmlSchemas.NfeAutorizacao.Envio.TVeiculo
            {
                placa = notaFiscal.Transporte.Veiculo.Placa,
                UF = TUfConversor.ToTUf(notaFiscal.Transporte.Veiculo.SiglaUF)
            };

            transp.Items = new object[] { veiculo };
            transp.ItemsElementName = new[] { XmlSchemas.NfeAutorizacao.Envio.ItemsChoiceType5.veicTransp };

            return transp;
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePag GetPagamento(NotaFiscal notaFiscal)
        {
            return notaFiscal.Pagamentos == null
                ? null
                : new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePag
                {
                    detPag = notaFiscal.Pagamentos.Select(pagamento => new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePagDetPag
                    {
                        vPag = pagamento.Valor.AsNumberFormattedString(),
                        tPag = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFePagDetPagTPag)(int)pagamento.FormaPagamento
                    }).ToArray()
                };
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIde GetIdentificacao(NotaFiscal notaFiscal)
        {
            var ide = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIde
            {
                cUF = notaFiscal.Identificacao.UF.ToTCodUfIBGE(),
                nNF = notaFiscal.Identificacao.Numero,
                cNF = notaFiscal.Identificacao.Chave.Codigo,
                natOp = notaFiscal.Identificacao.NaturezaOperacao,
                mod = (XmlSchemas.NfeAutorizacao.Envio.TMod)(int)notaFiscal.Identificacao.Modelo,
                serie = notaFiscal.Identificacao.Serie.ToString(),
                dhEmi = notaFiscal.Identificacao.DataHoraEmissao.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                tpNF = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeTpNF)(int)notaFiscal.Identificacao.TipoOperacao,
                idDest = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeIdDest)(int)notaFiscal.Identificacao.OperacaoDestino,
                cMunFG = notaFiscal.Identificacao.CodigoMunicipio,
                tpImp = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeTpImp)(int)notaFiscal.Identificacao.FormatoImpressao,
                tpEmis = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeTpEmis)(int)notaFiscal.Identificacao.TipoEmissao,
                tpAmb = (XmlSchemas.NfeAutorizacao.Envio.TAmb)(int)notaFiscal.Identificacao.Ambiente,
                finNFe = (XmlSchemas.NfeAutorizacao.Envio.TFinNFe)(int)notaFiscal.Identificacao.FinalidadeEmissao,
                indFinal = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeIndFinal)(int)notaFiscal.Identificacao.FinalidadeConsumidor,
                indPres = (XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeIdeIndPres)(int)notaFiscal.Identificacao.PresencaComprador,
                procEmi = (XmlSchemas.NfeAutorizacao.Envio.TProcEmi)(int)notaFiscal.Identificacao.ProcessoEmissao,
                verProc = notaFiscal.Identificacao.VersaoAplicativo,
                cDV = notaFiscal.Identificacao.Chave.DigitoVerificador.ToString()
            };

            if (!notaFiscal.IsContingency()) return ide;

            ide.dhCont = notaFiscal.Identificacao.DataHoraEntradaContigencia.ToString("yyyy-MM-ddTHH:mm:sszzz");
            ide.xJust = notaFiscal.Identificacao.JustificativaContigencia;

            return ide;
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeEmit GetEmitente(NotaFiscal notaFiscal)
        {
            return new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeEmit
            {
                Item = notaFiscal.Emitente.CNPJ,
                xNome = notaFiscal.Emitente.Nome,
                xFant = notaFiscal.Emitente.NomeFantasia,
                IE = notaFiscal.Emitente.InscricaoEstadual,
                IM = notaFiscal.Emitente.InscricaoMunicipal,
                CNAE = notaFiscal.Emitente.CNAE,
                CRT = notaFiscal.Emitente.CRT,
                enderEmit = new XmlSchemas.NfeAutorizacao.Envio.TEnderEmi
                {
                    xLgr = notaFiscal.Emitente.Endereco.Logradouro,
                    nro = notaFiscal.Emitente.Endereco.Numero,
                    xBairro = notaFiscal.Emitente.Endereco.Bairro,
                    cMun = notaFiscal.Emitente.Endereco.CodigoMunicipio,
                    xMun = notaFiscal.Emitente.Endereco.Municipio,
                    UF = TUfEmiConversor.TUfEmi(notaFiscal.Emitente.Endereco.UF),
                    CEP = notaFiscal.Emitente.Endereco.Cep,
                    cPais = XmlSchemas.NfeAutorizacao.Envio.TEnderEmiCPais.Item1058,
                    xPais = XmlSchemas.NfeAutorizacao.Envio.TEnderEmiXPais.Brasil,
                    fone = notaFiscal.Emitente.Telefone
                }
            };
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDest GetDestinatario(NotaFiscal notaFiscal)
        {
            var dest = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDest { Item = notaFiscal.Destinatario.Documento.Numero };

            switch (notaFiscal.Destinatario.TipoDestinatario)
            {
                case TipoDestinatario.PessoaFisica:
                    dest.ItemElementName = XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType3.CPF;
                    break;
                case TipoDestinatario.PessoaJuridica:
                    dest.ItemElementName = XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType3.CNPJ;
                    break;
                case TipoDestinatario.Estrangeiro:
                    dest.ItemElementName = XmlSchemas.NfeAutorizacao.Envio.ItemChoiceType3.idEstrangeiro;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            dest.xNome = notaFiscal.Destinatario.NomeRazao;

            if (notaFiscal.Destinatario.IsIsentoICMS)
                dest.indIEDest = XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDestIndIEDest.Item2;
            else
                dest.indIEDest = notaFiscal.Identificacao.Modelo == Modelo.Modelo65
                    ? XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDestIndIEDest.Item9
                    : XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDestIndIEDest.Item1;

            dest.IE = notaFiscal.Destinatario.InscricaoEstadual;
            dest.email = notaFiscal.Destinatario.Email;

            if (notaFiscal.Destinatario.Endereco != null)
            {
                dest.enderDest = new XmlSchemas.NfeAutorizacao.Envio.TEndereco
                {
                    xLgr = notaFiscal.Destinatario.Endereco.Logradouro,
                    nro = notaFiscal.Destinatario.Endereco.Numero,
                    xBairro = notaFiscal.Destinatario.Endereco.Bairro,
                    cMun = notaFiscal.Destinatario.Endereco.CodigoMunicipio,
                    xMun = notaFiscal.Destinatario.Endereco.Municipio,
                    UF = TUfDestConversor.TUf(notaFiscal.Destinatario.Endereco.UF),
                    CEP = notaFiscal.Destinatario.Endereco.Cep,
                    cPais = "1058",
                    xPais = "Brasil",
                    fone = notaFiscal.Destinatario.Telefone
                };
            }

            return dest;
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDet[] GetDetalhamentoProdutos(NotaFiscal notaFiscal)
        {
            var detList = new List<XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDet>();

            for (var i = 0; i < notaFiscal.Produtos.Count; i++)
            {
                var newDet = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDet
                {
                    prod = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetProd
                    {
                        cProd = notaFiscal.Produtos[i].Codigo,
                        cEAN = "SEM GTIN",
                        xProd = notaFiscal.Produtos[i].Descricao,
                        NCM = notaFiscal.Produtos[i].Ncm,
                        CEST = notaFiscal.Produtos[i].Cest,
                        uCom = notaFiscal.Produtos[i].UnidadeComercial,
                        qCom = notaFiscal.Produtos[i].QtdeUnidadeComercial.ToString(),
                        vUnCom = notaFiscal.Produtos[i].ValorUnidadeComercial.AsNumberFormattedString(),
                        vProd = notaFiscal.Produtos[i].ValorTotal.AsNumberFormattedString(),
                        cEANTrib = "SEM GTIN",
                        uTrib = notaFiscal.Produtos[i].UnidadeComercial,
                        qTrib = notaFiscal.Produtos[i].QtdeUnidadeComercial.ToString(),
                        vUnTrib = notaFiscal.Produtos[i].ValorUnidadeComercial.AsNumberFormattedString(),
                        CFOP = notaFiscal.Produtos[i].Cfop.ToString().Replace("Item", string.Empty),
                        indTot = XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetProdIndTot.Item1,
                        vFrete = notaFiscal.Produtos[i].Frete.ToPositiveDecimalAsStringOrNull(),
                        vOutro = notaFiscal.Produtos[i].Outros.ToPositiveDecimalAsStringOrNull(),
                        vSeg = notaFiscal.Produtos[i].Seguro.ToPositiveDecimalAsStringOrNull(),
                        vDesc = notaFiscal.Produtos[i].Desconto.ToPositiveDecimalAsStringOrNull()
                    }
                };

                // Tratamento de produtos específicos (combustíveis)
                if (notaFiscal.ProdutoÉCombustível(i))
                {
                    var comb = new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetProdComb
                    {
                        cProdANP = "210203001",
                        UFCons = TUfConversor.ToTUf(notaFiscal.Destinatario.Endereco.UF),
                        descANP = "GLP",
                        pGLP = "100.00",
                        vPart = (notaFiscal.Produtos[i].ValorUnidadeComercial / 13).ToString("F",
                            CultureInfo.InvariantCulture)
                    };

                    newDet.prod.uTrib = "KG";
                    newDet.prod.Items = new object[] { comb };
                }

                newDet.imposto = GetImposto(notaFiscal.Produtos[i]);
                newDet.nItem = (i + 1).ToString();

                detList.Add(newDet);
            }

            return detList.ToArray();
        }

        private static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeDetImposto GetImposto(Produto produto)
        {
            var directorFactory = new ImpostoCreatorFactory();
            var impostoFactory = new NfeDetImpostoFactory(directorFactory);
            return impostoFactory.CreateNfeDetImposto(produto.Impostos);
        }

        public static XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalICMSTot ConvertIcmsTotal(NotaFiscal notaFiscal)
        {
            notaFiscal.TotalNFe.IcmsTotal = IcmsTotal.CalculateIcmsTotal(notaFiscal.Produtos);

            return new XmlSchemas.NfeAutorizacao.Envio.TNFeInfNFeTotalICMSTot
            {
                vBC = notaFiscal.TotalNFe.IcmsTotal.BaseCalculo.AsNumberFormattedString(),
                vICMS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms.AsNumberFormattedString(),
                vICMSDeson = notaFiscal.TotalNFe.IcmsTotal.ValorTotalDesonerado.AsNumberFormattedString(),
                vFCPSTRet = notaFiscal.TotalNFe.IcmsTotal
                    .TotalFundoCombatePobrezaSubstituicaoTributariaRetidoAnteriormente.AsNumberFormattedString(),
                vProd = notaFiscal.TotalNFe.IcmsTotal.ValorTotalProdutos.AsNumberFormattedString(),
                vFrete = notaFiscal.TotalNFe.IcmsTotal.ValorTotalFrete.AsNumberFormattedString(),
                vSeg = notaFiscal.TotalNFe.IcmsTotal.ValorTotalSeguro.AsNumberFormattedString(),
                vDesc = notaFiscal.TotalNFe.IcmsTotal.ValorTotalDesconto.AsNumberFormattedString(),
                vOutro = notaFiscal.TotalNFe.IcmsTotal.TotalOutros.AsNumberFormattedString(),
                vFCPST = notaFiscal.TotalNFe.IcmsTotal.TotalFundoCombatePobrezaSubstituicaoTributaria
                    .AsNumberFormattedString(),
                vBCST = notaFiscal.TotalNFe.IcmsTotal.BaseCalculoST.AsNumberFormattedString(),
                vST = notaFiscal.TotalNFe.IcmsTotal.ValorTotalST.AsNumberFormattedString(),
                vFCP = notaFiscal.TotalNFe.IcmsTotal.TotalFundoCombatePobreza.AsNumberFormattedString(),
                vII = notaFiscal.TotalNFe.IcmsTotal.ValorTotalII.AsNumberFormattedString(),
                vIPI = notaFiscal.TotalNFe.IcmsTotal.ValorTotalIpi.AsNumberFormattedString(),
                vPIS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalPis.AsNumberFormattedString(),
                vCOFINS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalCofins.AsNumberFormattedString(),
                vNF = notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe.AsNumberFormattedString(),
                vIPIDevol = "0.00"
            };
        }
    }
}