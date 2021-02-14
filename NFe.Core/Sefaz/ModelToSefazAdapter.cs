using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NFe.Core.Extensions;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.NotasFiscais.Impostos.Icms;
using NFe.Core.NotasFiscais.ValueObjects;
using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.Utils.Conversores.Enums.Autorizacao;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core.Sefaz
{
    internal class ModelToSefazAdapter
    {
        public static TEnviNFe GetLoteNFe(NotaFiscal notaFiscal)
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

            var nfe = new TNFe {infNFe = infNFe};

            if (IsNfce(notaFiscal))
                nfe.infNFeSupl = new TNFeInfNFeSupl
                    {qrCode = "", urlChave = "http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx"};
            else
                nfe.infNFeSupl = null;

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

        private static bool IsNfce(NotaFiscal notaFiscal)
        {
            return notaFiscal.Identificacao.Modelo == Modelo.Modelo65;
        }

        private static TNFeInfNFeTotal GetTotal(NotaFiscal notaFiscal)
        {
            var total = new TNFeInfNFeTotal
            {
                ICMSTot = notaFiscal.TotalNFe.IcmsTotal == null ? null : ConvertIcmsTotal(notaFiscal),
                ISSQNtot = ConvertIssqn(notaFiscal),
                retTrib = ConvertTributosFederais(notaFiscal)
            };

            return total;
        }

        private static TNFeInfNFeTotalRetTrib ConvertTributosFederais(NotaFiscal notaFiscal)
        {
            var retencaoTributosFederais = notaFiscal.TotalNFe.RetencaoTributosFederais;
            if (retencaoTributosFederais == null) return null;

            return new TNFeInfNFeTotalRetTrib
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

        private static TNFeInfNFeTotalISSQNtot ConvertIssqn(NotaFiscal notaFiscal)
        {
            var issqnTotal = notaFiscal.TotalNFe.IssqnTotal;
            if (issqnTotal == null) return null;

            return new TNFeInfNFeTotalISSQNtot
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
                cRegTrib = (TNFeInfNFeTotalISSQNtotCRegTrib) (int) issqnTotal.RegimeEspecialTributacao
            };
        }

        public static TNFeInfNFeTotalICMSTot ConvertIcmsTotal(NotaFiscal notaFiscal)
        {
            notaFiscal.TotalNFe.IcmsTotal = CalculateIcmsTotal(notaFiscal.Produtos);

            return new TNFeInfNFeTotalICMSTot
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

        private static IcmsTotal CalculateIcmsTotal(IReadOnlyCollection<Produto> produtos)
        {
            var impostos = produtos.SelectMany(p => p.Impostos);
            var impostosIcms = impostos.Where(i => i is Icms);
            var icmsDesonerados = impostos.Where(i => i is IcmsDesonerado);
            var icmsRetidoAnteriormente = impostos.Where(i => i is IcmsSubstituicaoTributariaRetidoAnteiormente);
            var icmsSubstituicaoTributaria = impostos.Where(i => i is HasSubstituicaoTributaria);
            var impostosII = impostos.Where(i => i is II);
            var impostosIpi = impostos.Where(i => i is Ipi);
            var impostosPis = impostos.Where(i => i is Pis);
            var impostosCofins = impostos.Where(i => i is CofinsBase);

            var valorTotalST = icmsSubstituicaoTributaria.Sum(i =>
                (double) ((HasSubstituicaoTributaria) i).SubstituicaoTributaria.Valor);
            var valorTotalFCPST = icmsSubstituicaoTributaria.Sum(i =>
                (double) ((HasSubstituicaoTributaria) i).SubstituicaoTributaria.FundoCombatePobreza.Valor);
            var valorTotalFrete = produtos.Sum(p => p.Frete);
            var valorTotalSeguro = produtos.Sum(p => p.Seguro);
            var valorTotalOutros = produtos.Sum(p => p.Outros);
            var valorTotalII = impostosII.Sum(i => (double) ((II) i).Valor);
            var valorTotalIPI = impostosIpi.Sum(i => (double) ((Ipi) i).Valor);
            var valorTotalDesconto = produtos.Sum(p => p.Desconto);
            var valorTotalIcmsDesonerado = icmsDesonerados.Sum(i =>
            {
                var icmsDesonerado = (IcmsDesonerado) i;
                if (icmsDesonerado.Desoneracao != null)
                    return (double) icmsDesonerado.Desoneracao.ValorDesonerado;

                return 0;
            });
            var valorTotalProdutos = produtos.Sum(p => p.ValorTotal);
            var valorTotalBaseCalculoIcms = impostosIcms.Sum(i => (double) ((Icms) i).BaseCalculo);
            var valorTotalIcms = impostosIcms.Sum(i => (double) ((Icms) i).Valor);
            var valorFCPRetidoAnteriormentePorST =
                icmsRetidoAnteriormente.Sum(i => (double) ((HasFundoCombatePobreza) i).FundoCombatePobreza.Valor);
            var valorTotalBaseCalculoPorST = icmsSubstituicaoTributaria.Sum(i =>
                (double) ((HasSubstituicaoTributaria) i).SubstituicaoTributaria.BaseCalculo);
            var valorTotalFCP = impostosIcms.Where(i => i is HasFundoCombatePobreza)
                .Sum(i => (double) ((HasFundoCombatePobreza) i).FundoCombatePobreza.Valor);
            var valorTotalPis = impostosPis.Sum(i => (double) ((Pis) i).Valor);
            var valorTotalCofins = impostosCofins.Sum(i => (double) ((CofinsBase) i).Valor);
            var totalNFe = valorTotalProdutos
                           + valorTotalST
                           + valorTotalFCPST
                           + valorTotalFrete
                           + valorTotalSeguro
                           + valorTotalOutros
                           + valorTotalII
                           + valorTotalIPI
                           - valorTotalDesconto
                           - valorTotalIcmsDesonerado;

            var icmsTotal = new IcmsTotal
            {
                BaseCalculo = valorTotalBaseCalculoIcms,
                BaseCalculoST = valorTotalBaseCalculoPorST,
                TotalFundoCombatePobreza = valorTotalFCP,
                TotalFundoCombatePobrezaSubstituicaoTributaria = valorTotalFCPST,
                TotalFundoCombatePobrezaSubstituicaoTributariaRetidoAnteriormente = valorFCPRetidoAnteriormentePorST,
                ValorTotalFrete = valorTotalFrete,
                ValorTotalDesconto = valorTotalDesconto,
                ValorTotalDesonerado = valorTotalIcmsDesonerado,
                ValorTotalCofins = valorTotalCofins,
                ValorTotalNFe = totalNFe,
                ValorTotalIcms = valorTotalIcms,
                ValorTotalSeguro = valorTotalSeguro,
                ValorTotalPis = valorTotalPis,
                ValorTotalProdutos = valorTotalProdutos,
                ValorTotalST = valorTotalST,
                ValorTotalII = valorTotalII,
                ValorTotalIpi = valorTotalIPI,
                TotalOutros = valorTotalOutros
            };

            return icmsTotal;
        }

        private static TNFeInfNFeInfAdic GetInformacaoAdicional(NotaFiscal notaFiscal)
        {
            var infAdic = new TNFeInfNFeInfAdic
            {
                infCpl = notaFiscal.InfoAdicional.InfoAdicionalComplementar,
                infAdFisco = notaFiscal.InfoAdicional.InfoAdicionalFisco
            };

            return infAdic;
        }

        private static TNFeInfNFeTransp GetTransporte(NotaFiscal notaFiscal)
        {
            var transp = new TNFeInfNFeTransp
            {
                modFrete = (TNFeInfNFeTranspModFrete) (int) notaFiscal.Transporte.ModalidadeFrete
            };

            if (notaFiscal.Transporte.Transportadora == null)
                return transp;

            var transportadora = notaFiscal.Transporte.Transportadora;

            transp.transporta = new TNFeInfNFeTranspTransporta
            {
                Item = transportadora.CpfCnpj,
                ItemElementName = transportadora.CpfCnpj.Length == 14 ? ItemChoiceType6.CNPJ : ItemChoiceType6.CPF,
                xNome = transportadora.Nome,
                IE = transportadora.InscricaoEstadual,
                xEnder = transportadora.EnderecoCompleto,
                xMun = transportadora.Municipio,
                UF = TUfConversor.ToTUf(transportadora.SiglaUF),
                UFSpecified = true
            };

            if (notaFiscal.Transporte.Veiculo == null)
                return transp;

            var veiculo = new TVeiculo
            {
                placa = notaFiscal.Transporte.Veiculo.Placa,
                UF = TUfConversor.ToTUf(notaFiscal.Transporte.Veiculo.SiglaUF)
            };

            transp.Items = new object[] {veiculo};
            transp.ItemsElementName = new[] {ItemsChoiceType5.veicTransp};

            return transp;
        }

        private static TNFeInfNFePag GetPagamento(NotaFiscal notaFiscal)
        {
            return notaFiscal.Pagamentos == null
                ? null
                : new TNFeInfNFePag
                {
                    detPag = notaFiscal.Pagamentos.Select(pagamento => new TNFeInfNFePagDetPag
                    {
                        vPag = pagamento.Valor.AsNumberFormattedString(),
                        tPag = (TNFeInfNFePagDetPagTPag) (int) pagamento.FormaPagamento
                    }).ToArray()
                };
        }

        private static TNFeInfNFeIde GetIdentificacao(NotaFiscal notaFiscal)
        {
            var ide = new TNFeInfNFeIde
            {
                cUF = notaFiscal.Identificacao.UF.ToTCodUfIBGE(),
                nNF = notaFiscal.Identificacao.Numero,
                cNF = notaFiscal.Identificacao.Chave.Codigo,
                natOp = notaFiscal.Identificacao.NaturezaOperacao,
                mod = (TMod) (int) notaFiscal.Identificacao.Modelo,
                serie = notaFiscal.Identificacao.Serie.ToString(),
                dhEmi = notaFiscal.Identificacao.DataHoraEmissao.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                tpNF = (TNFeInfNFeIdeTpNF) (int) notaFiscal.Identificacao.TipoOperacao,
                idDest = (TNFeInfNFeIdeIdDest) (int) notaFiscal.Identificacao.OperacaoDestino,
                cMunFG = notaFiscal.Identificacao.CodigoMunicipio,
                tpImp = (TNFeInfNFeIdeTpImp) (int) notaFiscal.Identificacao.FormatoImpressao,
                tpEmis = (TNFeInfNFeIdeTpEmis) (int) notaFiscal.Identificacao.TipoEmissao,
                tpAmb = (TAmb) (int) notaFiscal.Identificacao.Ambiente,
                finNFe = (TFinNFe) (int) notaFiscal.Identificacao.FinalidadeEmissao,
                indFinal = (TNFeInfNFeIdeIndFinal) (int) notaFiscal.Identificacao.FinalidadeConsumidor,
                indPres = (TNFeInfNFeIdeIndPres) (int) notaFiscal.Identificacao.PresencaComprador,
                procEmi = (TProcEmi) (int) notaFiscal.Identificacao.ProcessoEmissao,
                verProc = notaFiscal.Identificacao.VersaoAplicativo,
                cDV = notaFiscal.Identificacao.Chave.DigitoVerificador.ToString()
            };

            if (!IsContingency(notaFiscal)) return ide;

            ide.dhCont = notaFiscal.Identificacao.DataHoraEntradaContigencia.ToString("yyyy-MM-ddTHH:mm:sszzz");
            ide.xJust = notaFiscal.Identificacao.JustificativaContigencia;

            return ide;
        }

        private static bool IsContingency(NotaFiscal notaFiscal)
        {
            return notaFiscal.Identificacao.TipoEmissao == TipoEmissao.ContigenciaNfce ||
                   notaFiscal.Identificacao.TipoEmissao == TipoEmissao.FsDa;
        }

        private static TNFeInfNFeEmit GetEmitente(NotaFiscal notaFiscal)
        {
            var emit = new TNFeInfNFeEmit
            {
                Item = notaFiscal.Emitente.CNPJ,
                xNome = notaFiscal.Emitente.Nome,
                xFant = notaFiscal.Emitente.NomeFantasia,
                IE = notaFiscal.Emitente.InscricaoEstadual,
                IM = notaFiscal.Emitente.InscricaoMunicipal,
                CNAE = notaFiscal.Emitente.CNAE,
                CRT = (TNFeInfNFeEmitCRT) (int) notaFiscal.Emitente.CRT,
                enderEmit = new TEnderEmi
                {
                    xLgr = notaFiscal.Emitente.Endereco.Logradouro,
                    nro = notaFiscal.Emitente.Endereco.Numero,
                    xBairro = notaFiscal.Emitente.Endereco.Bairro,
                    cMun = notaFiscal.Emitente.Endereco.CodigoMunicipio,
                    xMun = notaFiscal.Emitente.Endereco.Municipio,
                    UF = TUfEmiConversor.TUfEmi(notaFiscal.Emitente.Endereco.UF),
                    CEP = notaFiscal.Emitente.Endereco.Cep,
                    cPais = TEnderEmiCPais.Item1058,
                    xPais = TEnderEmiXPais.Brasil,
                    fone = notaFiscal.Emitente.Telefone
                }
            };

            return emit;
        }

        private static TNFeInfNFeDest GetDestinatario(NotaFiscal notaFiscal)
        {
            var dest = new TNFeInfNFeDest {Item = notaFiscal.Destinatario.Documento.Numero};

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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            dest.xNome = notaFiscal.Destinatario.NomeRazao;

            if (notaFiscal.Destinatario.IsIsentoICMS)
                dest.indIEDest = TNFeInfNFeDestIndIEDest.Item2;
            else
                dest.indIEDest = notaFiscal.Identificacao.Modelo == Modelo.Modelo65
                    ? TNFeInfNFeDestIndIEDest.Item9
                    : TNFeInfNFeDestIndIEDest.Item1;

            dest.IE = notaFiscal.Destinatario.InscricaoEstadual;
            dest.email = notaFiscal.Destinatario.Email;

            if (notaFiscal.Destinatario.Endereco != null)
                dest.enderDest = new TEndereco
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

            return dest;
        }

        // Volatilidade no preechimento do produto, usar polimorfismo + factory ou strategy
        private static TNFeInfNFeDet[] GetDetalhamentoProdutos(NotaFiscal notaFiscal)
        {
            var detList = new List<TNFeInfNFeDet>();

            for (var i = 0; i < notaFiscal.Produtos.Count; i++)
            {
                var newDet = new TNFeInfNFeDet
                {
                    prod = new TNFeInfNFeDetProd
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
                        indTot = TNFeInfNFeDetProdIndTot.Item1,
                        vFrete = notaFiscal.Produtos[i].Frete.ToPositiveDecimalAsStringOrNull(),
                        vOutro = notaFiscal.Produtos[i].Outros.ToPositiveDecimalAsStringOrNull(),
                        vSeg = notaFiscal.Produtos[i].Seguro.ToPositiveDecimalAsStringOrNull(),
                        vDesc = notaFiscal.Produtos[i].Desconto.ToPositiveDecimalAsStringOrNull()
                    }
                };

                // Tratamento de produtos específicos (combustíveis)
                if (ProdutoÉCombustível(notaFiscal, i))
                {
                    var comb = new TNFeInfNFeDetProdComb
                    {
                        cProdANP = "210203001",
                        UFCons = TUfConversor.ToTUf(notaFiscal.Destinatario.Endereco.UF),
                        descANP = "GLP",
                        pGLP = "100.00",
                        vPart = (notaFiscal.Produtos[i].ValorUnidadeComercial / 13).ToString("F",
                            CultureInfo.InvariantCulture)
                    };

                    newDet.prod.uTrib = "KG";
                    newDet.prod.Items = new object[] {comb};
                }

                newDet.imposto = GetImposto(notaFiscal.Produtos[i]);
                newDet.nItem = (i + 1).ToString();

                detList.Add(newDet);
            }

            return detList.ToArray();
        }

        private static bool ProdutoÉCombustível(NotaFiscal notaFiscal, int i)
        {
            return notaFiscal.Identificacao.Modelo != Modelo.Modelo65 && notaFiscal.Produtos[i].Ncm.Equals("27111910");
        }

        private static TNFeInfNFeDetImposto GetImposto(Produto produto)
        {
            var directorFactory = new ImpostoCreatorFactory();
            var impostoFactory = new NfeDetImpostoFactory(directorFactory);
            var nfeDetImposto = impostoFactory.CreateNfeDetImposto(produto.Impostos);

            return nfeDetImposto;
        }
    }
}