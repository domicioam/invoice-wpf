using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using NFe.Core.Domain.Services;
using NFe.Core.Domain.Services.Emissor;
using NFe.Core.Domain.Services.ICMS;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Domain.Services.Pagto;
using NFe.Core.Domain.Services.Transp;
using NFe.Core.Factory;
using NFe.Core.Model;
using NFe.Core.Model.Dest;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Servicos;
using NFe.Core.TO;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores.Enums;
using NFe.Core.Utils.QrCode;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using NFe.Repository;
using NFe.Repository.Entities;
using NFe.Repository.Entitities.Enums;
using NFe.Repository.Repositories;
using Retorno = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using TNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFe;
using TUf = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TUf;
using TUfEmi = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TUfEmi;

namespace NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao
{
   public class NFeAutorizacaoNormal
   {
      public string GerarPreVisualizacao(NotaFiscal notaFiscal)
      {
         return XmlUtil.GerarXmlLoteNFe(notaFiscal, "http://www.portalfiscal.inf.br/nfe");
      }

      public bool IsNotaFiscalValida(NotaFiscal notaFiscal, string cscId, string csc)
      {
         string qrCode = "";

         string refUri = "#NFe" + notaFiscal.Identificacao.Chave;
         string digVal = "";
         string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

         X509Certificate2 certificado;

         var certificadoEntity = new CertificadoRepository(new NFeContext()).GetCertificado();

         if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
         {
            certificado = CertificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
         }
         else
         {
            certificado = CertificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
         }

         var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>", string.Empty); ;
         XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

         TNFe nfe = null;
         string newNodeXml = string.Empty;

         try
         {
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), notaFiscal.Emitente.Endereco.UF);

            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
            {
               qrCode = QrCodeUtil.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario, digVal, notaFiscal.Identificacao.Ambiente,
                           notaFiscal.Identificacao.DataHoraEmissao, notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe.ToString("F", CultureInfo.InvariantCulture),
                           notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms.ToString("F", CultureInfo.InvariantCulture), cscId, csc, notaFiscal.Identificacao.TipoEmissao);

               newNodeXml = node.InnerXml.Replace("<qrCode />", "<qrCode>" + qrCode + "</qrCode>");
            }
            else
            {
               newNodeXml = node.InnerXml;
            }

            var document = new XmlDocument();
            document.LoadXml(newNodeXml);
            node = document.DocumentElement;

            TEnviNFe lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(node.OuterXml);
            nfe = lote.NFe[0];

            ValidadorXml.ValidarXml(node.OuterXml, "enviNFe_v4.00.xsd");

            return true;
         }
         catch (Exception)
         {
            return false;
         }
      }

      /** <exception cref="Exception"/>
      * <summary>Responsável pelo envio da Nota Fiscal para a SEFAZ.</summary>
      */
      internal async Task<int> EnviarNotaFiscalAsync(NotaFiscal notaFiscal, string cscId, string csc)
        {
            string qrCode = "";
            TNFe nfe = null;
            string newNodeXml = string.Empty;
            int idNotaCopiaSeguranca = 0;
            NotaFiscalEntity notaFiscalEntity = null;

            string refUri = "#NFe" + notaFiscal.Identificacao.Chave;
            string digVal = "";
            string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            X509Certificate2 certificado;

            var certificadoEntity = new CertificadoRepository(new NFeContext()).GetCertificado();

            certificado = EscolherCertificado(certificadoEntity);

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>", string.Empty);
            XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

            try
            {
                var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), notaFiscal.Emitente.Endereco.UF);

                if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                {
                    //NFC-e tem QrCode obrigatório
                    qrCode = QrCodeUtil.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario, digVal, notaFiscal.Identificacao.Ambiente,
                                notaFiscal.Identificacao.DataHoraEmissao, notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe.ToString("F", CultureInfo.InvariantCulture),
                                notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms.ToString("F", CultureInfo.InvariantCulture), cscId, csc, notaFiscal.Identificacao.TipoEmissao);

                    newNodeXml = node.InnerXml.Replace("<qrCode />", "<qrCode>" + qrCode + "</qrCode>");
                }
                else
                {
                    newNodeXml = node.InnerXml;
                }

                var document = new XmlDocument();
                document.LoadXml(newNodeXml);
                node = document.DocumentElement;

                TEnviNFe lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(node.OuterXml);
                nfe = lote.NFe[0];

                var servico = ServiceFactory.GetService(notaFiscal.Identificacao.Modelo, notaFiscal.Identificacao.Ambiente, Factory.Servico.AUTORIZACAO, codigoUF, certificado);
                var client = (NFeAutorizacao4.NFeAutorizacao4SoapClient)servico.SoapClient;

                //salvar nota PreEnvio aqui
                notaFiscal.Identificacao.Status = NFe.Repository.Status.PENDENTE;

                var notaFiscalService = new NotaFiscalService();

                idNotaCopiaSeguranca = await notaFiscalService.SalvarNotaFiscalPendenteAsync(notaFiscal, XmlUtil.GerarNfeProcXml(nfe, qrCode), notaFiscal.Identificacao.Ambiente);

                XmlNode result = client.nfeAutorizacaoLote(node);
                TRetEnviNFe retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);
                XmlSchemas.NfeAutorizacao.Retorno.TProtNFe protocolo = (XmlSchemas.NfeAutorizacao.Retorno.TProtNFe)retorno.Item; //existem dois valores possíveis de retorno (esse aqui só vale para lote com 1 nota)

                if (protocolo.infProt.cStat.Equals("100"))
                {
                    notaFiscalEntity = await notaFiscalService.GetNotaFiscalByIdAsync(idNotaCopiaSeguranca, false);
                    notaFiscalEntity.Status = (int)Status.ENVIADA;
                    notaFiscalEntity.DataAutorizacao = DateTime.ParseExact(protocolo.infProt.dhRecbto, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

                    notaFiscalEntity.Protocolo = protocolo.infProt.nProt;
                    string xmlNFeProc = XmlUtil.GerarNfeProcXml(nfe, qrCode, protocolo);

                    await notaFiscalService.SalvarAsync(notaFiscalEntity, xmlNFeProc);
                }
                else
                {
                    if (protocolo.infProt.xMotivo.Contains("Duplicidade"))
                    {
                        notaFiscalEntity = await CorrigirNotaDuplicada(notaFiscal, qrCode, nFeNamespaceName, certificado, nfe, idNotaCopiaSeguranca, notaFiscalEntity);
                    }
                    else
                    {
                        //Nota continua com status pendente nesse caso
                        XmlUtil.SalvarXmlNFeComErro(notaFiscal, node);
                        string mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", protocolo.infProt.xMotivo);
                        throw new ArgumentException(mensagem);
                    }
                }

                notaFiscal.QrCodeUrl = qrCode;
                notaFiscal.Identificacao.Status = Status.ENVIADA;
                notaFiscal.DhAutorizacao = notaFiscalEntity.DataAutorizacao.ToString("dd/MM/yyyy HH:mm:ss");
                notaFiscal.DataHoraAutorização = notaFiscalEntity.DataAutorizacao;
                notaFiscal.ProtocoloAutorizacao = notaFiscalEntity.Protocolo;
                return idNotaCopiaSeguranca;
            }
            catch (Exception e)
            {
                var codigoUF = notaFiscal.Identificacao.UF;
                var ambiente = notaFiscal.Identificacao.Ambiente;

                if (e.InnerException is WebException)
                {
                    throw new Exception("Serviço indisponível ou sem conexão com a internet.", e.InnerException);
                }

                try
                {
                    notaFiscalEntity = await VerificarSeNotaFoiEnviada(notaFiscal, cscId, csc, qrCode, nfe, idNotaCopiaSeguranca, notaFiscalEntity, nFeNamespaceName, certificado);
                }
                catch (Exception retornoConsultaException)
                {
                    EscreverLogErro(e);
                    EscreverLogErro(retornoConsultaException);
                    XmlUtil.SalvarXmlNFeComErro(notaFiscal, node);
                    throw retornoConsultaException;
                }

                notaFiscal.QrCodeUrl = qrCode;
                notaFiscal.Identificacao.Status = Status.ENVIADA;
                notaFiscal.DhAutorizacao = notaFiscalEntity.DataAutorizacao.ToString("dd/MM/yyyy HH:mm:ss");
                notaFiscal.DataHoraAutorização = notaFiscalEntity.DataAutorizacao;
                notaFiscal.ProtocoloAutorizacao = notaFiscalEntity.Protocolo;
                return idNotaCopiaSeguranca;
            }
        }

        private async Task<NotaFiscalEntity> VerificarSeNotaFoiEnviada(NotaFiscal notaFiscal, string cscId, string csc, string qrCode, TNFe nfe, int idNotaCopiaSeguranca, NotaFiscalEntity notaFiscalEntity, string nFeNamespaceName, X509Certificate2 certificado)
        {
            var retornoConsulta = NFeConsulta.ConsultarNotaFiscal(notaFiscal.Identificacao.Chave, notaFiscal.Emitente.Endereco.CodigoUF,
                certificado, notaFiscal.Identificacao.Ambiente, notaFiscal.Identificacao.Modelo);

            if (retornoConsulta.IsEnviada)
            {
                var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                var protDeserialized = (XmlSchemas.NfeAutorizacao.Retorno.TProtNFe)XmlUtil.Deserialize<XmlSchemas.NfeAutorizacao.Retorno.TProtNFe>(protSerialized);

                var notaFiscalService = new NotaFiscalService();

                notaFiscalEntity = await notaFiscalService.GetNotaFiscalByIdAsync(idNotaCopiaSeguranca, false);
                notaFiscalEntity.Status = (int)Status.ENVIADA;
                notaFiscalEntity.DataAutorizacao = retornoConsulta.DhAutorizacao;

                notaFiscalEntity.Protocolo = retornoConsulta.Protocolo.infProt.nProt;
                string xmlNfeProc = XmlUtil.GerarNfeProcXml(nfe, qrCode, protDeserialized);

                await notaFiscalService.SalvarAsync(notaFiscalEntity, xmlNfeProc);
            }

            return notaFiscalEntity;
        }

        private static X509Certificate2 EscolherCertificado(CertificadoEntity certificadoEntity)
        {
            X509Certificate2 certificado;
            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
            {
                certificado = CertificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            }
            else
            {
                certificado = CertificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
            }

            return certificado;
        }

        private static async Task<NotaFiscalEntity> CorrigirNotaDuplicada(NotaFiscal notaFiscal, string qrCode, string nFeNamespaceName, X509Certificate2 certificado, TNFe nfe, int idNotaCopiaSeguranca, NotaFiscalEntity notaFiscalEntity)
        {
            var retornoConsulta = NFeConsulta.ConsultarNotaFiscal(notaFiscal.Identificacao.Chave, notaFiscal.Emitente.Endereco.CodigoUF, certificado,
                notaFiscal.Identificacao.Ambiente, notaFiscal.Identificacao.Modelo);

            var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
            var protDeserialized = (XmlSchemas.NfeAutorizacao.Retorno.TProtNFe)XmlUtil.Deserialize<XmlSchemas.NfeAutorizacao.Retorno.TProtNFe>(protSerialized);

            var notaFiscalService = new NotaFiscalService();

            notaFiscalEntity = await notaFiscalService.GetNotaFiscalByIdAsync(idNotaCopiaSeguranca, false);
            notaFiscalEntity.Status = (int)Status.ENVIADA;
            notaFiscalEntity.DataAutorizacao = retornoConsulta.DhAutorizacao;

            notaFiscalEntity.Protocolo = retornoConsulta.Protocolo.infProt.nProt;
            string xmlNFeProc = XmlUtil.GerarNfeProcXml(nfe, qrCode, protDeserialized);

            await notaFiscalService.SalvarAsync(notaFiscalEntity, xmlNFeProc);
            return notaFiscalEntity;
        }

        public NotaFiscal GetNotaFiscalFromNfeProcXML(string xml)
      {
            Retorno.TNfeProc nfeProc = ((Retorno.TNfeProc)XmlUtil.Deserialize<Retorno.TNfeProc>(xml));
            Retorno.TNFe nfe = nfeProc.NFe;

         var emitente = GetEmitente(nfe);
         var destinatario = GetDestinatario(nfe);
         var pagamentos = GetPagamentos(nfe);
         var identificacao = GetIdentificacao(nfe, emitente, pagamentos);
         var transporte = GetTransporte(nfe);
         var totalNFe = GetTotalNFe(nfe);
         var produtos = GetProdutos(nfe);
         var infoAdicional = GetInfoAdicional(produtos);

            NotaFiscal notaFiscal = new NotaFiscal(emitente, destinatario, identificacao, transporte, totalNFe, infoAdicional, produtos, pagamentos, xml);
            notaFiscal.DhAutorizacao = nfeProc.protNFe.infProt.dhRecbto;
            notaFiscal.DataHoraAutorização = DateTime.ParseExact(nfeProc.protNFe.infProt.dhRecbto, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

            notaFiscal.ProtocoloAutorizacao = nfeProc.protNFe.infProt.nProt;

            return notaFiscal;
      }

      private void EscreverLogErro(Exception retornoConsultaException)
      {
         string sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmissorNFeDir");

         if (!Directory.Exists(sDirectory))
         {
            Directory.CreateDirectory(sDirectory);
         }

         using (FileStream stream = File.Create(Path.Combine(sDirectory, "log1.txt")))
         {
            using (StreamWriter writer = new StreamWriter(stream))
            {
               writer.WriteLine(retornoConsultaException.ToString());
            }
         }
      }

      private void PreencherLogNotaComErroDesconhecido(Exception e)
      {
         string sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmissorNFeDir");

         if (!Directory.Exists(sDirectory))
         {
            Directory.CreateDirectory(sDirectory);
         }

         using (FileStream stream = File.Create(Path.Combine(sDirectory, "log3.txt")))
         {
            using (StreamWriter writer = new StreamWriter(stream))
            {
               writer.WriteLine(e.ToString() + "\n" + e.Message);
            }
         }

         using (FileStream stream = File.Create(Path.Combine(sDirectory, "log4.txt")))
         {
            using (StreamWriter writer = new StreamWriter(stream))
            {
               if (e.InnerException != null)
                  writer.WriteLine(e.InnerException.ToString() + "\n" + e.InnerException.Message);
            }
         }
      }

      private RetornoNotaFiscal PreencheMensagemRetorno(XmlSchemas.NfeAutorizacao.Retorno.TProtNFe protocolo, string urlQrCode, TNFe nfe)
      {
         RetornoNotaFiscal mensagem = new RetornoNotaFiscal()
         {
            CodigoStatus = protocolo.infProt.cStat,
            Motivo = protocolo.infProt.xMotivo,
            Protocolo = protocolo.infProt.nProt,
            UrlQrCode = urlQrCode,
            DataAutorizacao = protocolo.infProt.dhRecbto,
            Xml = XmlUtil.GerarNfeProcXml(nfe, urlQrCode, protocolo)
         };

         return mensagem;
      }

      private Emissor GetEmitente(Retorno.TNFe nfe)
      {
         var nfeEmit = nfe.infNFe.emit;
         string regimeTributario = "";

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
         }

         string uf = TUfEmiConversor.ToUfString((TUfEmi)(int)nfeEmit.enderEmit.UF);
         var endereco = new Endereco(nfeEmit.enderEmit.xLgr, nfeEmit.enderEmit.nro, nfeEmit.enderEmit.xBairro, nfeEmit.enderEmit.xMun, nfeEmit.enderEmit.CEP, uf);
         return new Emissor(nfeEmit.xNome, nfeEmit.xFant, nfeEmit.Item, nfeEmit.IE, nfeEmit.IE, nfeEmit.CNAE, regimeTributario, endereco, nfeEmit.enderEmit.fone);
      }

      private Destinatario GetDestinatario(Retorno.TNFe nfe)
      {
         var nfeDest = nfe.infNFe.dest;

         if (nfeDest != null)
         {
            TipoDestinatario tipoDestinatario = new TipoDestinatario();

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
            }

            if (nfeDest.enderDest != null)
            {
               string uf = TUfDestConversor.ToUfString((TUf)(int)nfeDest.enderDest.UF);
               var endereco = new Model.Dest.Endereco(nfeDest.enderDest.xLgr, nfeDest.enderDest.nro, nfeDest.enderDest.xBairro, nfeDest.enderDest.xMun, nfeDest.enderDest.CEP, uf);

               Ambiente ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;
               Modelo modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;

               return new Destinatario(ambiente, modelo, nfeDest.enderDest.fone, nfeDest.email, endereco, tipoDestinatario, nfeDest.IE, nomeRazao: nfeDest.xNome, documento: nfeDest.Item);
            }
            else
            {
               Ambiente ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;
               Modelo modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;

               return new Destinatario(ambiente, modelo, null, nfeDest.email, null, tipoDestinatario, nfeDest.IE, nomeRazao: nfeDest.xNome, documento: nfeDest.Item);
            }
         }

         return null;
      }

      private IdentificacaoNFe GetIdentificacao(Retorno.TNFe nfe, Emissor emitente, List<Pagamento> pagamentos)
      {
         var uf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
         var dataEmissao = DateTime.ParseExact(nfe.infNFe.ide.dhEmi, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
         var ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;
         var modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;
         var tipoEmissao = nfe.infNFe.ide.tpEmis == Retorno.TNFeInfNFeIdeTpEmis.Item9 ? TipoEmissao.ContigenciaNfce : TipoEmissao.Normal; //Isso aqui vai me dar problema
         var formatoImpressao = (FormatoImpressao)(int)nfe.infNFe.ide.tpImp;
         var isImpressaoBobina = formatoImpressao == FormatoImpressao.Nfce;
         var finalidadeConsumidor = (FinalidadeConsumidor)(int)nfe.infNFe.ide.indFinal;
         var documentoDanfe = finalidadeConsumidor == FinalidadeConsumidor.ConsumidorFinal ? "CPF" : "CNPJ";
         var finalidadeNFe = (FinalidadeEmissao)(int)nfe.infNFe.ide.finNFe;

         var ide = new IdentificacaoNFe(uf, dataEmissao, emitente.CNPJ, modelo, Int32.Parse(nfe.infNFe.ide.serie), nfe.infNFe.ide.nNF, tipoEmissao, ambiente, emitente, nfe.infNFe.ide.natOp,
             finalidadeNFe, isImpressaoBobina, PresencaComprador.Presencial, documentoDanfe);

         if (nfe.infNFe.ide.tpEmis == Retorno.TNFeInfNFeIdeTpEmis.Item9)
         {
            ide.DataHoraEntradaContigencia = DateTime.ParseExact(nfe.infNFe.ide.dhCont, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
            ide.JustificativaContigencia = nfe.infNFe.ide.xJust;
         }

         return ide;
      }

      private Transporte GetTransporte(Retorno.TNFe nfe)
      {
         var transportadoraNFe = nfe.infNFe.transp.transporta;
         if (transportadoraNFe != null)
         {
            var uf = TUfConversor.ToSiglaUf((TUf)(int)transportadoraNFe.UF);

            Modelo modelo = nfe.infNFe.ide.mod == Retorno.TMod.Item55 ? Modelo.Modelo55 : Modelo.Modelo65;
            var transportadora = new Transportadora(transportadoraNFe.Item, transportadoraNFe.xEnder, transportadoraNFe.IE, transportadoraNFe.xMun, uf, transportadoraNFe.xNome);

            Veiculo veiculo = null;

            if (nfe.infNFe.transp.Items.Length > 0)
            {
               var veiculoNFe = (Retorno.TVeiculo)nfe.infNFe.transp.Items[0];
               veiculo = new Veiculo(veiculoNFe.placa, TUfConversor.ToSiglaUf(veiculoNFe.UF));
            }

            return new Transporte(modelo, transportadora, veiculo);
         }
         else if (nfe.infNFe.transp.modFrete == Retorno.TNFeInfNFeTranspModFrete.Item9)
         {
            return new Transporte(Modelo.Modelo65, null, null);
         }

         return null;
      }

      private TotalNFe GetTotalNFe(Retorno.TNFe nfe)
      {
         var infNfeTotal = nfe.infNFe.total.ICMSTot;

         var totalNFe = new TotalNFe();
         totalNFe.IcmsTotal = new IcmsTotal();
         var icmsTotal = totalNFe.IcmsTotal;

         icmsTotal.BaseCalculo = Double.Parse(infNfeTotal.vBC, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalIcms = Double.Parse(infNfeTotal.vICMS, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalDesonerado = Double.Parse(infNfeTotal.vICMSDeson, CultureInfo.InvariantCulture);
         icmsTotal.BaseCalculoST = Double.Parse(infNfeTotal.vBCST, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalST = Double.Parse(infNfeTotal.vST, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalProdutos = Double.Parse(infNfeTotal.vProd, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalFrete = Double.Parse(infNfeTotal.vFrete, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalSeguro = Double.Parse(infNfeTotal.vSeg, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalDesconto = Double.Parse(infNfeTotal.vDesc, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalII = Double.Parse(infNfeTotal.vII, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalIpi = Double.Parse(infNfeTotal.vIPI, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalPis = Double.Parse(infNfeTotal.vPIS, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalCofins = Double.Parse(infNfeTotal.vCOFINS, CultureInfo.InvariantCulture);
         icmsTotal.ValorDespesasAcessorias = Double.Parse(infNfeTotal.vOutro, CultureInfo.InvariantCulture);
         icmsTotal.ValorTotalNFe = Double.Parse(infNfeTotal.vNF, CultureInfo.InvariantCulture);

         return totalNFe;
      }

      private InfoAdicional GetInfoAdicional(List<Produto> produtos)
      {
         return new InfoAdicional(produtos);
      }

      private List<Produto> GetProdutos(Retorno.TNFe nfe)
      {
         var produtos = new List<Produto>();

         Ambiente ambiente = (Ambiente)(int)nfe.infNFe.ide.tpAmb;

         foreach (var det in nfe.infNFe.det)
         {
            var icmsDet = (Retorno.TNFeInfNFeDetImpostoICMS)det.imposto.Items[0];

            Imposto icms = new Imposto() { TipoImposto = TipoImposto.Icms, Aliquota = 0 };

            if (icmsDet.Item is Retorno.TNFeInfNFeDetImpostoICMSICMS60)
            {
               var icms60 = (Retorno.TNFeInfNFeDetImpostoICMSICMS60)icmsDet.Item;
               icms.CST = TabelaIcmsCst.IcmsCobradoAnteriormentePorST;
            }
            else if (icmsDet.Item is Retorno.TNFeInfNFeDetImpostoICMSICMS40)
            {
               var icms40 = (Retorno.TNFeInfNFeDetImpostoICMSICMS40)icmsDet.Item;
               icms.CST = TabelaIcmsCst.Isenta;
            }

            var pisNT = (Retorno.TNFeInfNFeDetImpostoPISPISNT)det.imposto.PIS.Item;
            Imposto pis = new Imposto() { TipoImposto = TipoImposto.PIS, Aliquota = 0, CST = pisNT.CST.ToString().Replace("Item", string.Empty) };

            GrupoImpostos grupoImpostos = new GrupoImpostos()
            {
               CFOP = det.prod.CFOP.ToString().Replace("Item", string.Empty),
               Impostos = new List<Imposto> { icms, pis }
            };

            var newProduto = new Produto(grupoImpostos, 0, det.prod.CFOP.ToString().Replace("Item", string.Empty), det.prod.cProd, det.prod.xProd, det.prod.NCM,
                Int32.Parse(det.prod.qCom), det.prod.uCom, Double.Parse(det.prod.vUnCom, CultureInfo.InvariantCulture), 0, ambiente == Ambiente.Producao);

            newProduto.Cest = det.prod.CEST;

            produtos.Add(newProduto);
         }

         return produtos;
      }

      private List<Pagamento> GetPagamentos(Retorno.TNFe nfe)
      {
         var pagamentos = new List<Pagamento>();
         var pagNFe = nfe.infNFe.pag;

         if (pagNFe != null)
         {
            foreach (var pag in pagNFe.detPag)
            {

               var formaPagamento = (FormaPagamento)(int)pag.tPag;
               var pagamentoTexto = string.Empty;

               switch (formaPagamento)
               {
                  case FormaPagamento.CartaoCredito:
                     pagamentoTexto = "Cartão de Crédito";
                     break;
                  case FormaPagamento.CartaoDebito:
                     pagamentoTexto = "Cartão de Débito";
                     break;
                  case FormaPagamento.Cheque:
                     pagamentoTexto = "Cheque";
                     break;
                  case FormaPagamento.Dinheiro:
                     pagamentoTexto = "Dinheiro";
                     break;
                  case FormaPagamento.SemPagamento:
                     pagamentoTexto = "Sem Pagamento";
                     break;
               }

               pagamentos.Add(new Pagamento(formaPagamento, Double.Parse(pag.vPag, CultureInfo.InvariantCulture)));
            }

            return pagamentos;
         }

         return null;
      }
   }

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
