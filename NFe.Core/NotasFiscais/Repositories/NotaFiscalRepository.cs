using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using NFe.Core;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Entitities.Enums;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.Core.NotasFiscais.Repositories;
using NFe.Core.NotasFiscais.ValueObjects;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using Imposto = NFe.Core.NotasFiscais.Entities.Imposto;
using Retorno = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;

namespace NFe.Repository.Repositories
{
    public class NotaFiscalRepository : INotaFiscalRepository
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfiguracaoRepository _configuracaoRepository;

        public NotaFiscalRepository(IConfiguracaoRepository configuracaoRepository)
        {
            _configuracaoRepository = configuracaoRepository;
        }

        public void ExcluirNota(string chave)
        {
            var notaFiscalEntity = GetNotaFiscalByChave(chave);
            ExcluirNota(notaFiscalEntity.Numero, notaFiscalEntity.Serie, notaFiscalEntity.Modelo);

            try
            {
                XmlFileHelper.DeleteXmlFile(notaFiscalEntity);
            }
            catch (Exception e)
            {
                log.Error(e);
                throw new Exception("Não foi possível remover o xml de nota fiscal: " + e.Message);
            }
        }

        public int Salvar(NotaFiscalEntity notafiscal)
        {
            using (var context = new NFeContext())
            {
                if (notafiscal.Id == 0)
                    context.Entry(notafiscal).State = EntityState.Added;
                else
                    context.Entry(notafiscal).State = EntityState.Modified;

                context.SaveChanges();
                return notafiscal.Id;
            }
        }

        public List<NotaFiscalEntity> GetAll()
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();

                if (config == null) return null;

                return context.NotaFiscal.ToList();
            }
        }

        public List<NotaFiscalEntity> Take(int quantity)
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();

                if (config == null) return null;

                return context.NotaFiscal.Take(quantity).ToList();
            }
        }

        public List<NotaFiscalEntity> GetNotasPendentes()
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();
                return context.NotaFiscal
                    .Where(n => n.Status == (int)Status.PENDENTE).ToList();
            }
        }

        public NotaFiscalEntity GetNotaFiscalById(int idNotaFiscalDb, bool isLoadXmlData)
        {
            using (var context = new NFeContext())
            {
                var notaTo = context.NotaFiscal.FirstOrDefault(n => n.Id == idNotaFiscalDb);

                if (isLoadXmlData) notaTo.LoadXml();

                return notaTo;
            }
        }

        public IEnumerable<NotaFiscalEntity> GetNotasFiscaisPorMesAno(DateTime mesAno)
        {
            using (var context = new NFeContext())
            {
                return context.NotaFiscal.Where(n =>
                    n.DataEmissao.Month.Equals(mesAno.Month) && n.DataEmissao.Year == mesAno.Year).ToList();
            }
        }

        public void ExcluirNota(string numero, string serie, string modelo)
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();
                var nota = context.NotaFiscal.First(c =>
                    c.Numero.Equals(numero) && c.Serie.Equals(serie) && c.Modelo.Equals(modelo));
                context.NotaFiscal.Remove(nota);
                context.SaveChanges();
            }
        }

        public virtual NotaFiscalEntity GetNotaFiscalByChave(string chave)
        {
            using (var context = new NFeContext())
            {
                return context.NotaFiscal.FirstOrDefault(n => n.Chave == chave);
            }
        }

        public List<NotaFiscalEntity> GetNotasContingencia()
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();
                return context.NotaFiscal
                    .Where(n => n.Status == (int) Status.CONTINGENCIA).ToList();
            }
        }

        public NotaFiscalEntity GetPrimeiraNotaEmitidaEmContingencia(DateTime dataHoraContingencia, DateTime now)
        {
            using (var context = new NFeContext())
            {
                return context.NotaFiscal.Where(n => n.Status == 3 && n.DataEmissao >= dataHoraContingencia)
                    .OrderBy(n => n.DataEmissao).FirstOrDefault();
            }
        }

        public NotaFiscalEntity GetNota(string numero, string serie, string modelo)
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();
                return context.NotaFiscal.FirstOrDefault(c =>
                    c.Numero.Equals(numero) && c.Serie.Equals(serie) && c.Modelo.Equals(modelo));
            }
        }

        public IEnumerable<NotaFiscalEntity> Take(int pageSize, int page)
        {
            int skip = (page - 1) * pageSize;

            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();

                if (config == null) return null;

                return context.NotaFiscal
                    .OrderByDescending(n => n.Id)
                                .Skip(skip)
                                .Take(pageSize)
                                .ToList();
            }
        }

        public int SalvarNotaFiscalPendente(NotaFiscal notaFiscal, string xml)
        {
            var notaFiscalEntity = new NotaFiscalEntity
            {
                UfDestinatario = notaFiscal.Destinatario?.Endereco != null
                    ? notaFiscal.Destinatario.Endereco.UF
                    : notaFiscal.Emitente.Endereco.UF,
                Destinatario = notaFiscal.Destinatario == null
                    ? "CONSUMIDOR NÃO IDENTIFICADO"
                    : notaFiscal.Destinatario.NomeRazao,
                DocumentoDestinatario = notaFiscal.Destinatario?.Documento.Numero,
                Status = notaFiscal.Identificacao.Status.GetIntValue(),
                Chave = notaFiscal.Identificacao.Chave.ToString(),
                DataEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                Modelo = notaFiscal.Identificacao.Modelo == Modelo.Modelo55 ? "55" : "65",
                Serie = notaFiscal.Identificacao.Serie.ToString(),
                TipoEmissao = notaFiscal.Identificacao.TipoEmissao.ToString(),
                ValorDesconto = notaFiscal.TotalNFe.IcmsTotal.ValorTotalDesconto,
                ValorDespesas = notaFiscal.TotalNFe.IcmsTotal.TotalOutros,
                ValorFrete = notaFiscal.TotalNFe.IcmsTotal.ValorTotalFrete,
                ValorICMS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms,
                ValorProdutos = notaFiscal.ValorTotalProdutos,
                ValorSeguro = notaFiscal.TotalNFe.IcmsTotal.ValorTotalSeguro,
                ValorTotal = notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe,
                Numero = notaFiscal.Identificacao.Numero
            };

            return Salvar(notaFiscalEntity, xml);
        }

        public virtual int Salvar(NotaFiscalEntity notaFiscalEntity, string xml)
        {
            try
            {
                if (notaFiscalEntity.Status != (int)Status.CANCELADA)
                    notaFiscalEntity.XmlPath = XmlFileHelper.SaveXmlFile(notaFiscalEntity, xml);
                return Salvar(notaFiscalEntity);
            }
            catch(Exception e)
            {
                log.Error(e);
                try
                {
                    XmlFileHelper.DeleteXmlFile(notaFiscalEntity);
                    throw;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    throw new Exception("Não foi possível remover o xml de nota fiscal: " + e.Message);
                }
            }
        }

        public virtual int Salvar(NotaFiscal notaFiscal, string xml)
        {
            var notaFiscalEntity = new NotaFiscalEntity
            {
                UfDestinatario = notaFiscal.Destinatario?.Endereco != null ? notaFiscal.Destinatario.Endereco.UF : notaFiscal.Emitente.Endereco.UF,
                Destinatario = notaFiscal.Destinatario == null ? "CONSUMIDOR NÃO IDENTIFICADO" : notaFiscal.Destinatario.NomeRazao,
                DocumentoDestinatario = notaFiscal.Destinatario?.Documento.Numero,
                Status = notaFiscal.Identificacao.Status.GetIntValue(),
                Chave = notaFiscal.Identificacao.Chave.ToString(),
                DataEmissao = notaFiscal.Identificacao.DataHoraEmissao,
                Modelo = notaFiscal.Identificacao.Modelo == Modelo.Modelo55 ? "55" : "65",
                Serie = notaFiscal.Identificacao.Serie.ToString(),
                TipoEmissao = notaFiscal.Identificacao.TipoEmissao.ToString(),
                ValorDesconto = notaFiscal.TotalNFe.IcmsTotal.ValorTotalDesconto,
                ValorDespesas = notaFiscal.TotalNFe.IcmsTotal.TotalOutros,
                ValorFrete = notaFiscal.TotalNFe.IcmsTotal.ValorTotalFrete,
                ValorICMS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms,
                ValorProdutos = notaFiscal.ValorTotalProdutos,
                ValorSeguro = notaFiscal.TotalNFe.IcmsTotal.ValorTotalSeguro,
                ValorTotal = notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe,
                Numero = notaFiscal.Identificacao.Numero,
                DataAutorizacao = notaFiscal.DataHoraAutorização,
                Protocolo = notaFiscal.ProtocoloAutorizacao
            };

            if (notaFiscalEntity.Status != (int)Status.CANCELADA)
                notaFiscalEntity.XmlPath = XmlFileHelper.SaveXmlFile(notaFiscalEntity, xml);

            return Salvar(notaFiscalEntity);
        }

        public List<NotaFiscalEntity> GetNotasFiscaisPorMesAno(DateTime periodo, bool isLoadXmlData)
        {
            var notasdb = GetNotasFiscaisPorMesAno(periodo);

            var notas = new List<NotaFiscalEntity>();

            foreach (var notaDb in notasdb)
            {
                var notaTo = notaDb;

                if (isLoadXmlData) notaTo.LoadXml();

                notas.Add(notaTo);
            }

            return notas;
        }

        public List<NotaFiscal> GetNotasFiscaisPorPeriodo(DateTime periodoInicial,
            DateTime periodoFinal, bool isLoadXmlData)
        {
            var notas = new List<NotaFiscal>();

            var notasDb = GetNotasFiscaisPorPeriodo(periodoInicial, periodoFinal);

            foreach (var notaDb in notasDb)
            {
                var notaTo = notaDb;

                var xml = notaTo.LoadXml();
                var notaFiscal = GetNotaFiscalFromNfeProcXml(xml);

                notas.Add(notaFiscal);
            }

            return notas;
        }

        public IEnumerable<NotaFiscalEntity> GetNotasFiscaisPorPeriodo(DateTime periodoInicial, DateTime periodoFinal)
        {
            using (var context = new NFeContext())
            {
                var config = context.Configuracao.FirstOrDefault();
                return context.NotaFiscal.Where(n =>
                    n.DataEmissao >= periodoInicial && n.DataEmissao <= periodoFinal &&
                    n.Status == (int) Status.ENVIADA).ToList();
            }
        }

        public List<NotaFiscalEntity> GetNotasPendentes(bool isLoadXmlData)
        {
            var notasPendentesDb = GetNotasPendentes();
            var notasPendentes = new List<NotaFiscalEntity>();

            foreach (var notaPendenteDb in notasPendentesDb)
            {
                var notaPendenteTo = notaPendenteDb;

                if (isLoadXmlData) notaPendenteTo.LoadXml();

                notasPendentes.Add(notaPendenteTo);
            }

            return notasPendentes;
        }

        public NotaFiscal GetNotaFiscalFromNfeProcXml(string xml)
        {
            var nfeProc = (Retorno.TNfeProc)XmlUtil.Deserialize<Retorno.TNfeProc>(xml);
            var nfe = nfeProc.NFe;

            var emitente = GetEmitente(nfe);
            var destinatario = GetDestinatario(nfe);
            var pagamentos = GetPagamentos(nfe);
            var identificacao = GetIdentificacao(nfe, emitente);
            var transporte = GetTransporte(nfe);
            var totalNFe = GetTotalNFe(nfe);
            var produtos = GetProdutos(nfe);
            var infoAdicional = GetInfoAdicional(produtos);
            var qrCode = nfe.infNFeSupl?.qrCode;

            var notaFiscal = new NotaFiscal(emitente, destinatario, identificacao, transporte, totalNFe, infoAdicional,
                produtos, pagamentos);

            if (nfeProc.protNFe.infProt != null)
            {
                notaFiscal.DhAutorizacao = nfeProc.protNFe.infProt.dhRecbto;
                notaFiscal.DataHoraAutorização = DateTime.ParseExact(nfeProc.protNFe.infProt.dhRecbto,
                    "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

                notaFiscal.ProtocoloAutorizacao = nfeProc.protNFe.infProt.nProt;
            }

            notaFiscal.QrCodeUrl = qrCode;

            return notaFiscal;
        }

        public void SalvarXmlNFeComErro(NotaFiscal notaFiscal, XmlNode node)
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

        private Emissor GetEmitente(Retorno.TNFe nfe)
        {
            var nfeEmit = nfe.infNFe.emit;
            string regimeTributario;

            switch (nfeEmit.CRT)
            {
                case Retorno.TNFeInfNFeEmitCRT.Item1:
                    regimeTributario = "Simples Nacional";
                    break;
                case Retorno.TNFeInfNFeEmitCRT.Item2:
                    regimeTributario = "Simples Nacional Excesso Receita Bruta";
                    break;
                case Retorno.TNFeInfNFeEmitCRT.Item3:
                    regimeTributario = "Regime Normal";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var uf = TUfEmiConversor.ToUfString((TUfEmi)(int)nfeEmit.enderEmit.UF);
            var endereco = new Core.Endereco(nfeEmit.enderEmit.xLgr, nfeEmit.enderEmit.nro, nfeEmit.enderEmit.xBairro,
                nfeEmit.enderEmit.xMun, nfeEmit.enderEmit.CEP, uf);
            return new Emissor(nfeEmit.xNome, nfeEmit.xFant, nfeEmit.Item, nfeEmit.IE, nfeEmit.IE, nfeEmit.CNAE,
                regimeTributario, endereco, nfeEmit.enderEmit.fone);
        }

        private Destinatario GetDestinatario(Retorno.TNFe nfe)
        {
            var nfeDest = nfe.infNFe.dest;

            if (nfeDest == null) 
                return null;

            TipoDestinatario tipoDestinatario;

            switch (nfeDest.ItemElementName)
            {
                case Retorno.ItemChoiceType3.CPF:
                    tipoDestinatario = TipoDestinatario.PessoaFisica;
                    break;
                case Retorno.ItemChoiceType3.CNPJ:
                    tipoDestinatario = TipoDestinatario.PessoaJuridica;
                    break;
                case Retorno.ItemChoiceType3.idEstrangeiro:
                    tipoDestinatario = TipoDestinatario.Estrangeiro;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (nfeDest.enderDest != null)
            {
                var uf = TUfDestConversor.ToUfString((TUf)(int)nfeDest.enderDest.UF);
                var endereco = new Core.NotasFiscais.Endereco(nfeDest.enderDest.xLgr, nfeDest.enderDest.nro,
                    nfeDest.enderDest.xBairro, nfeDest.enderDest.xMun, nfeDest.enderDest.CEP, uf);

                var ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;
                var modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;

                return new Destinatario(ambiente, modelo, nfeDest.enderDest.fone, nfeDest.email, endereco,
                    tipoDestinatario, nfeDest.IE, nomeRazao: nfeDest.xNome, documento: new Documento(nfeDest.Item));
            }
            else
            {
                var ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;
                var modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;

                return new Destinatario(ambiente, modelo, null, nfeDest.email, null, tipoDestinatario, nfeDest.IE,
                    nomeRazao: nfeDest.xNome, documento: new Documento(nfeDest.Item));
            }

        }

        private IdentificacaoNFe GetIdentificacao(Retorno.TNFe nfe, Emissor emitente)
        {
            var uf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            var dataEmissao = DateTime.ParseExact(nfe.infNFe.ide.dhEmi, "yyyy-MM-ddTHH:mm:sszzz",
                CultureInfo.InvariantCulture);
            var ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;
            var modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;
            var tipoEmissao = nfe.infNFe.ide.tpEmis == Retorno.TNFeInfNFeIdeTpEmis.Item9
                ? TipoEmissao.ContigenciaNfce
                : TipoEmissao.Normal; //Isso aqui vai me dar problema
            var formatoImpressao = (FormatoImpressao)(int)nfe.infNFe.ide.tpImp;
            var isImpressaoBobina = formatoImpressao == FormatoImpressao.Nfce;
            var finalidadeConsumidor = (FinalidadeConsumidor)(int)nfe.infNFe.ide.indFinal;
            var documentoDanfe = finalidadeConsumidor == FinalidadeConsumidor.ConsumidorFinal ? "CPF" : "CNPJ";
            var finalidadeNFe = (FinalidadeEmissao)(int)nfe.infNFe.ide.finNFe;

            var ide = new IdentificacaoNFe(uf, dataEmissao, emitente.CNPJ, modelo, int.Parse(nfe.infNFe.ide.serie),
                nfe.infNFe.ide.nNF, tipoEmissao, ambiente, emitente, nfe.infNFe.ide.natOp,
                finalidadeNFe, isImpressaoBobina, PresencaComprador.Presencial, documentoDanfe);

            if (nfe.infNFe.ide.tpEmis != Retorno.TNFeInfNFeIdeTpEmis.Item9) return ide;

            ide.DataHoraEntradaContigencia = DateTime.ParseExact(nfe.infNFe.ide.dhCont, "yyyy-MM-ddTHH:mm:sszzz",
                CultureInfo.InvariantCulture);
            ide.JustificativaContigencia = nfe.infNFe.ide.xJust;

            return ide;
        }

        private Transporte GetTransporte(Retorno.TNFe nfe)
        {
            var transportadoraNFe = nfe.infNFe.transp.transporta;
            if (transportadoraNFe != null)
            {
                var uf = TUfConversor.ToSiglaUf((TUf)(int)transportadoraNFe.UF);

                var modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;
                var transportadora = new Transportadora(transportadoraNFe.Item, transportadoraNFe.xEnder,
                    transportadoraNFe.IE, transportadoraNFe.xMun, uf, transportadoraNFe.xNome);

                if (nfe.infNFe.transp.Items.Length <= 0) return new Transporte(modelo, transportadora, null);
                var veiculoNFe = (Retorno.TVeiculo)nfe.infNFe.transp.Items[0];
                var veiculo = new Veiculo(veiculoNFe.placa, TUfConversor.ToSiglaUf(veiculoNFe.UF));

                return new Transporte(modelo, transportadora, veiculo);
            }

            return nfe.infNFe.transp.modFrete == Retorno.TNFeInfNFeTranspModFrete.Item9
                ? new Transporte(Modelo.Modelo65, null, null)
                : null;
        }

        private TotalNFe GetTotalNFe(Retorno.TNFe nfe)
        {
            var infNfeTotal = nfe.infNFe.total.ICMSTot;

            var totalNFe = new TotalNFe { IcmsTotal = new IcmsTotal() };
            var icmsTotal = totalNFe.IcmsTotal;

            icmsTotal.BaseCalculo = double.Parse(infNfeTotal.vBC, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalIcms = double.Parse(infNfeTotal.vICMS, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalDesonerado = double.Parse(infNfeTotal.vICMSDeson, CultureInfo.InvariantCulture);
            icmsTotal.BaseCalculoST = double.Parse(infNfeTotal.vBCST, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalST = double.Parse(infNfeTotal.vST, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalProdutos = double.Parse(infNfeTotal.vProd, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalFrete = double.Parse(infNfeTotal.vFrete, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalSeguro = double.Parse(infNfeTotal.vSeg, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalDesconto = double.Parse(infNfeTotal.vDesc, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalII = double.Parse(infNfeTotal.vII, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalIpi = double.Parse(infNfeTotal.vIPI, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalPis = double.Parse(infNfeTotal.vPIS, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalCofins = double.Parse(infNfeTotal.vCOFINS, CultureInfo.InvariantCulture);
            icmsTotal.TotalOutros = double.Parse(infNfeTotal.vOutro, CultureInfo.InvariantCulture);
            icmsTotal.ValorTotalNFe = double.Parse(infNfeTotal.vNF, CultureInfo.InvariantCulture);

            return totalNFe;
        }

        private InfoAdicional GetInfoAdicional(List<Produto> produtos)
        {
            return new InfoAdicional(produtos);
        }

        private List<Produto> GetProdutos(Retorno.TNFe nfe)
        {
            var produtos = new List<Produto>();

            var ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;

            foreach (var det in nfe.infNFe.det)
            {
                var icmsDet = (Retorno.TNFeInfNFeDetImpostoICMS)det.imposto.Items[0];

                var icms = new Imposto { TipoImposto = TipoImposto.Icms, Aliquota = 0 };

                var icms60 = icmsDet.Item as Retorno.TNFeInfNFeDetImpostoICMSICMS60;
                icms.Origem = icms60.orig.ToOrigem();
                if (icms60 != null)
                {
                    icms.CST = TabelaIcmsCst.IcmsCobradoAnteriormentePorST;
                }
                else if (icmsDet.Item is Retorno.TNFeInfNFeDetImpostoICMSICMS40)
                {
                    icms.CST = TabelaIcmsCst.Isenta;
                }

                var pisNt = (Retorno.TNFeInfNFeDetImpostoPISPISNT)det.imposto.PIS.Item;
                var pis = new Imposto
                {
                    TipoImposto = TipoImposto.PIS,
                    Aliquota = 0,
                    CST = pisNt.CST.ToString().Replace("Item", string.Empty)
                };

                var impostos = new Impostos(new List<Imposto> { icms, pis});

                double.TryParse(det.prod.vSeg, NumberStyles.Float, CultureInfo.InvariantCulture, out double seguro);
                double.TryParse(det.prod.vOutro, NumberStyles.Float, CultureInfo.InvariantCulture, out double outros);
                double.TryParse(det.prod.vFrete, NumberStyles.Float, CultureInfo.InvariantCulture, out double frete);

                var newProduto = new Produto(impostos, 
                    0, 
                    det.prod.CFOP.Replace("Item", string.Empty),
                    det.prod.cProd, det.prod.xProd, det.prod.NCM,
                    int.Parse(det.prod.qCom), 
                    det.prod.uCom,
                    double.Parse(det.prod.vUnCom, CultureInfo.InvariantCulture), 
                    0, 
                    ambiente == Ambiente.Producao,
                    frete,
                    seguro,
                    outros)
                {
                    Cest = det.prod.CEST
                };


                produtos.Add(newProduto);
            }

            return produtos;
        }

        private List<Pagamento> GetPagamentos(Retorno.TNFe nfe)
        {
            var pagamentos = new List<Pagamento>();
            var pagNFe = nfe.infNFe.pag;

            if (pagNFe == null) return null;

            foreach (var pag in pagNFe.detPag)
            {
                var formaPagamento = (FormaPagamento)(int)pag.tPag;

                switch (formaPagamento)
                {
                    case FormaPagamento.CartaoCredito:
                        break;
                    case FormaPagamento.CartaoDebito:
                        break;
                    case FormaPagamento.Cheque:
                        break;
                    case FormaPagamento.Dinheiro:
                        break;
                    case FormaPagamento.SemPagamento:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                pagamentos.Add(new Pagamento(formaPagamento, double.Parse(pag.vPag, CultureInfo.InvariantCulture)));
            }

            return pagamentos;
        }
    }
}