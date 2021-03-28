using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Domain.Services;
using NFe.Core.Domain.Services.Emissor;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Factory;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Servicos;
using NFe.Core.TO;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.QrCode;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using NFe.Core.XmlSchemas.NfeRetAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeRetAutorizacao.Retorno;
using NFe.Repository;
using NFe.Repository.Repositories;

namespace NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao
{
    public class NFeAutorizacaoContingencia
    {
        private const string mensagemErro = "Tentativa de transmissão de notas em contingência falhou. Serviço continua indisponível.";
        private bool _isFirstTimeResending;
        private bool _isFirstTimeRecheckingRecipts;
        private static NFeAutorizacaoNormal _nfeAutorizacaoNormal = new NFeAutorizacaoNormal();

        internal async Task<int> EnviarNotaContingencia(NotaFiscal notaFiscal, string cscId, string csc)
        {
            TNFe nfe = null;
            string qrCode = string.Empty;
            string newNodeXml = string.Empty;
            string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
            string digVal = string.Empty;

            int idNotaCopiaSeguranca = 0;

            var config = ConfiguracaoService.GetConfiguracao();

            notaFiscal.Identificacao.DataHoraEntradaContigencia = config.DataHoraEntradaContingencia;
            notaFiscal.Identificacao.JustificativaContigencia = config.JustificativaContingencia;
            notaFiscal.Identificacao.TipoEmissao = notaFiscal.Identificacao.Modelo == Modelo.Modelo65 ? TipoEmissao.ContigenciaNfce : TipoEmissao.FsDa;
            notaFiscal.CalcularChave();

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

            if (notaFiscal.Identificacao.Ambiente == Domain.Services.Identificacao.Ambiente.Homologacao)
            {
                notaFiscal.Produtos[0].Descricao = "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";
            }

            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>", string.Empty);
            XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

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
                newNodeXml = node.InnerXml.Replace("<infNFeSupl><qrCode /></infNFeSupl>", "");
            }

            var document = new XmlDocument();
            document.LoadXml(newNodeXml);
            node = document.DocumentElement;

            TEnviNFe lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(node.OuterXml);
            nfe = lote.NFe[0];

            //salvar nota PreEnvio aqui
            notaFiscal.Identificacao.Status = NFe.Repository.Status.CONTINGENCIA;

            var notaFiscalService = new NotaFiscalService();

            idNotaCopiaSeguranca = await notaFiscalService.SalvarNotaFiscalPendenteAsync(notaFiscal, XmlUtil.GerarNfeProcXml(nfe, qrCode), notaFiscal.Identificacao.Ambiente);

            var notaFiscalEntity = await notaFiscalService.GetNotaFiscalByIdAsync(idNotaCopiaSeguranca, false);
            notaFiscalEntity.Status = (int)Status.CONTINGENCIA;
            string nfeProcXml = XmlUtil.GerarNfeProcXml(nfe, qrCode);

            await notaFiscalService.SalvarAsync(notaFiscalEntity, nfeProcXml);
            notaFiscal.QrCodeUrl = qrCode;
            return idNotaCopiaSeguranca;
        }

        public async Task<List<string>> TransmitirNotasFiscalEmContingencia() //Chamar esse método quando o serviço voltar
        {
            List<string> erros = new List<string>();

            var notaFiscalService = new NotaFiscalService();

            var notas = notaFiscalService.GetNotasContingencia();

            var config = ConfiguracaoService.GetConfiguracao();
            var notasNFe = new List<string>();
            var notasNFCe = new List<string>();

            foreach (var nota in notas)
            {
                string xml = await nota.LoadXmlAsync();

                if (nota.Modelo.Equals("55"))
                {
                    notasNFe.Add(xml);
                }
                else
                {
                    notasNFCe.Add(xml);
                }
            }

            try
            {
                if (notasNFCe.Count() != 0)
                {
                    erros = await new NFeAutorizacaoContingencia().TransmitirConsultarLoteContingenciaAsync(config, notasNFCe, Modelo.Modelo65);
                }

                if (notasNFe.Count() != 0)
                {
                    erros = await new NFeAutorizacaoContingencia().TransmitirConsultarLoteContingenciaAsync(config, notasNFe, Modelo.Modelo55);
                }
            }
            catch (Exception e)
            {
                string sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmissorNFeDir");

                if (!Directory.Exists(sDirectory))
                {
                    Directory.CreateDirectory(sDirectory);
                }

                using (FileStream stream = File.Create(Path.Combine(sDirectory, "logTransmitirContingencia.txt")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(e.ToString());
                    }
                }

                return null;
            }

            return erros;
        }

        private async Task<List<string>> TransmitirConsultarLoteContingenciaAsync(ConfiguracaoEntity config, List<string> notasNFCe, Modelo modelo)
        {
            var retornoTransmissao = TransmitirLoteNotasFiscaisContingencia(notasNFCe, modelo);

            if (retornoTransmissao.TipoMensagem == TipoMensagem.ErroValidacao)
            {
                return new List<string>() { retornoTransmissao.Mensagem };
            }
            else if (retornoTransmissao.TipoMensagem == TipoMensagem.ServicoIndisponivel)
            {
                return new List<string>() { mensagemErro };
            }

            int tempoEspera = Int32.Parse(retornoTransmissao.RetEnviNFeInfRec.tMed) * 1000;
            List<string> erros = new List<string>();
            Thread.Sleep(tempoEspera);
            var resultadoConsulta = ConsultarReciboLoteContingencia(retornoTransmissao.RetEnviNFeInfRec.nRec, modelo);

            if (resultadoConsulta == null)
            {
                return new List<string>() { mensagemErro };
            }

            foreach (var resultado in resultadoConsulta)
            {
                var ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;
                var nota = new NotaFiscalService().GetNotaFiscalByChave(resultado.Chave);

                if (resultado.CodigoStatus == "100")
                {
                    nota.DataAutorizacao = DateTime.ParseExact(resultado.DataAutorizacao, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
                    nota.Protocolo = resultado.Protocolo;
                    nota.Status = (int)NFe.Repository.Status.ENVIADA;

                    string xml = await nota.LoadXmlAsync();
                    xml = xml.Replace("<protNFe />", resultado.Xml);

                    var notaFiscalService = new NotaFiscalService();

                    await notaFiscalService.SalvarAsync(nota, xml);
                }
                else
                {
                    if (resultado.Motivo.Contains("Duplicidade"))
                    {
                        X509Certificate2 certificado;
                        var certificadoEntity = new CertificadoRepository(new NFeContext()).GetCertificado();
                        var emitente = EmissorService.GetEmissor();

                        if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                        {
                            certificado = CertificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                                RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
                        }
                        else
                        {
                            certificado = CertificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
                        }

                        var retornoConsulta = NFeConsulta.ConsultarNotaFiscal
                            (
                                nota.Chave,
                                emitente.Endereco.CodigoUF,
                                certificado,
                                config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao,
                                nota.Modelo.Equals("65") ? Modelo.Modelo65 : Modelo.Modelo55
                            );

                        if (retornoConsulta.IsEnviada)
                        {
                            var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, string.Empty)
                            .Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                            .Replace("TProtNFe", "protNFe");

                            protSerialized = Regex.Replace(protSerialized, "<infProt (.*?)>", "<infProt>");

                            nota.DataAutorizacao = retornoConsulta.DhAutorizacao;
                            nota.Protocolo = retornoConsulta.Protocolo.infProt.nProt;
                            nota.Status = (int)NFe.Repository.Status.ENVIADA;

                            string xml = await nota.LoadXmlAsync();
                            xml = xml.Replace("<protNFe />", protSerialized);

                            var notaFiscalService = new NotaFiscalService();

                            await notaFiscalService.SalvarAsync(nota, xml);
                        }
                        else
                        {
                            erros.Add(string.Format("Modelo: {0} Nota: {1} Série: {2} \nMotivo: {3}", nota.Modelo, nota.Numero, nota.Serie, resultado.Motivo)); //O que fazer com essas mensagens de erro?
                        }
                    }
                    else
                    {
                        erros.Add(string.Format("Modelo: {0} Nota: {1} Série: {2} \nMotivo: {3}", nota.Modelo, nota.Numero, nota.Serie, resultado.Motivo)); //O que fazer com essas mensagens de erro?
                    }
                }
            }

            return erros;
        }

        private List<RetornoNotaFiscal> ConsultarReciboLoteContingencia(string nRec, Modelo modelo)
        {
            var config = ConfiguracaoService.GetConfiguracao();
            X509Certificate2 certificado = null;

            var certificadoEntity = CertificadoService.GetCertificado();

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
            {
                certificado = CertificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            }
            else
            {
                certificado = CertificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
            }

            var consultaRecibo = new TConsReciNFe();
            consultaRecibo.versao = "4.00";
            consultaRecibo.tpAmb = config.IsProducao ? XmlSchemas.NfeRetAutorizacao.Envio.TAmb.Item1 : XmlSchemas.NfeRetAutorizacao.Envio.TAmb.Item2;
            consultaRecibo.nRec = nRec;

            string parametroXML = XmlUtil.Serialize(consultaRecibo, "http://www.portalfiscal.inf.br/nfe");

            var node = new XmlDocument();
            node.LoadXml(parametroXML);

            Ambiente ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), EmissorService.GetEmissor().Endereco.UF);

            try
            {
                var servico = ServiceFactory.GetService(modelo, ambiente, Factory.Servico.RetAutorizacao, codigoUF, certificado);
                var client = (NFeRetAutorizacao4.NFeRetAutorizacao4SoapClient)servico.SoapClient;

                var result = client.nfeRetAutorizacaoLote(node);

                TRetConsReciNFe retorno = (TRetConsReciNFe)XmlUtil.Deserialize<TRetConsReciNFe>(result.OuterXml);
                List<RetornoNotaFiscal> retornoConsultaList = new List<RetornoNotaFiscal>();

                foreach (var protNFe in retorno.protNFe)
                {
                    var retornoConsultaNota = new RetornoNotaFiscal();

                    retornoConsultaNota.Chave = protNFe.infProt.chNFe;
                    retornoConsultaNota.CodigoStatus = protNFe.infProt.cStat;
                    retornoConsultaNota.DataAutorizacao = protNFe.infProt.dhRecbto;
                    retornoConsultaNota.Motivo = protNFe.infProt.xMotivo;
                    retornoConsultaNota.Protocolo = protNFe.infProt.nProt;
                    retornoConsultaNota.Xml = XmlUtil.Serialize(protNFe, string.Empty)
                        .Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                        .Replace("TProtNFe", "protNFe")
                        .Replace("<infProt xmlns=\"http://www.portalfiscal.inf.br/nfe\">", "<infProt>");

                    retornoConsultaList.Add(retornoConsultaNota);
                }

                return retornoConsultaList;
            }
            catch (Exception)
            {
                if (!_isFirstTimeRecheckingRecipts)
                {
                    _isFirstTimeRecheckingRecipts = true;
                    return ConsultarReciboLoteContingencia(nRec, modelo);
                }
                else
                {
                    _isFirstTimeRecheckingRecipts = false;
                    return null;
                }
            }
        }

        private MensagemRetornoTransmissaoNotasContingencia TransmitirLoteNotasFiscaisContingencia(List<string> nfeList, Modelo modelo)
        {
            var lote = new TEnviNFe();
            lote.idLote = "999999"; //qual a regra pra gerar o id?
            lote.indSinc = TEnviNFeIndSinc.Item0; //apenas uma nota no lote
            lote.versao = "4.00";
            lote.NFe = new TNFe[1];
            lote.NFe[0] = new TNFe(); //Gera tag <NFe /> vazia para usar no replace

            string parametroXML = XmlUtil.Serialize(lote, "http://www.portalfiscal.inf.br/nfe");
            parametroXML = parametroXML.Replace("<NFe />", XmlUtil.GerarXmlListaNFe(nfeList)).Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            var document = new XmlDocument();
            document.LoadXml(parametroXML);
            var node = document.DocumentElement;

            var config = ConfiguracaoService.GetConfiguracao();

            Ambiente ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), EmissorService.GetEmissor().Endereco.UF);
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

            try
            {
                var servico = ServiceFactory.GetService(modelo, ambiente, Factory.Servico.AUTORIZACAO, codigoUF, certificado);
                var client = (NFeAutorizacao4.NFeAutorizacao4SoapClient)servico.SoapClient;

                XmlNode result = client.nfeAutorizacaoLote(node);
                TRetEnviNFe retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);

                return new MensagemRetornoTransmissaoNotasContingencia()
                {
                    RetEnviNFeInfRec = (TRetEnviNFeInfRec)retorno.Item,
                    TipoMensagem = TipoMensagem.Sucesso
                };
            }
            catch (Exception)
            {
                if (!_isFirstTimeResending)
                {
                    _isFirstTimeResending = true;
                    return TransmitirLoteNotasFiscaisContingencia(nfeList, modelo);
                }
                else
                {
                    _isFirstTimeResending = false;

                    return new MensagemRetornoTransmissaoNotasContingencia()
                    {
                        TipoMensagem = TipoMensagem.ServicoIndisponivel
                    };
                }
            }
        }

        private RetornoNotaFiscal PreencheMensagemRetornoContingencia(string urlQrCode, TNFe nfe)
        {
            RetornoNotaFiscal mensagem = new RetornoNotaFiscal()
            {
                UrlQrCode = urlQrCode,
                Xml = XmlUtil.GerarNfeProcXml(nfe, urlQrCode)
            };

            return mensagem;
        }
    }
}
