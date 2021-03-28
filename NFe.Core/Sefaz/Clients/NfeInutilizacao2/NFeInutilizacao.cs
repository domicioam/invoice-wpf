using System;
using AutoMapper;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.NFeInutilizacao4;
using NFe.Core.NotaFiscal;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores;
using Envio = NFe.Core.XmlSchemas.NfeInutilizacao2.Envio;
using Retorno = NFe.Core.XmlSchemas.NfeInutilizacao2.Retorno;

namespace NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2
{
    public class NFeInutilizacao
    {
        private ICertificadoService _certificadoService;
        private IServiceFactory _serviceFactory;

        public NFeInutilizacao(ICertificadoService certificadoService, IServiceFactory serviceFactory)
        {
            _certificadoService = certificadoService;
            _serviceFactory = serviceFactory;
        }

        internal MensagemRetornoInutilizacao InutilizarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, Ambiente ambiente, string cnpjEmitente, Modelo modeloNota,
            string serie, string numeroInicial, string numeroFinal)
        {
            var inutNFe = new Envio.TInutNFe();
            inutNFe.versao = "4.00";

            var infInut = new Envio.TInutNFeInfInut();
            infInut.tpAmb = (Envio.TAmb)(int)ambiente;
            infInut.xServ = Envio.TInutNFeInfInutXServ.INUTILIZAR;
            infInut.cUF = (Envio.TCodUfIBGE)(int)UfToTCOrgaoIBGEConversor.GetTCOrgaoIBGE(ufEmitente);
            infInut.ano = DateTime.Now.ToString("yy");
            infInut.CNPJ = cnpjEmitente;
            infInut.mod = (Envio.TMod)(int)modeloNota;
            infInut.serie = serie;
            infInut.nNFIni = numeroInicial;
            infInut.nNFFin = numeroFinal;
            infInut.xJust = "Não usada, quebra de sequência.";

            var cUF = infInut.cUF.ToString().Replace("Item", string.Empty);
            var modelo = modeloNota.ToString().Replace("Modelo", string.Empty);

            infInut.Id = "ID" + cUF + infInut.ano + cnpjEmitente + modelo + int.Parse(serie).ToString("D3") + int.Parse(numeroInicial).ToString("D9") + int.Parse(numeroFinal).ToString("D9");

            inutNFe.infInut = infInut;
            var xml = XmlUtil.Serialize(inutNFe, "http://www.portalfiscal.inf.br/nfe");
            var certificado = _certificadoService.GetX509Certificate2();
            var node = AssinaturaDigital.AssinarInutilizacao(xml, "#" + infInut.Id, certificado);

            var servico = _serviceFactory.GetService(modeloNota, Servico.INUTILIZACAO, codigoUf, certificado);

            var client = (NFeInutilizacao4SoapClient)servico.SoapClient;

            var result = client.nfeInutilizacaoNF(node);

            var retorno = (Retorno.TRetInutNFe)XmlUtil.Deserialize<Retorno.TRetInutNFe>(result.OuterXml);

            if (retorno.infInut.cStat.Equals("102"))
            {
                var procInut = new Retorno.ProcInut.TProcInutNFe();

                var procSerialized = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ProcInutNFe versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\">"
                    + node.OuterXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                    + result.OuterXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                    + "</ProcInutNFe>";

                return new MensagemRetornoInutilizacao()
                {
                    IdInutilizacao = infInut.Id,
                    Status = Status.SUCESSO,
                    Mensagem = retorno.infInut.xMotivo,
                    DataInutilizacao = retorno.infInut.dhRecbto,
                    MotivoInutilizacao = infInut.xJust,
                    ProtocoloInutilizacao = retorno.infInut.nProt,
                    Xml = procSerialized
                };
            }
            else
            {
                return new MensagemRetornoInutilizacao()
                {
                    Status = Status.ERRO,
                    Mensagem = retorno.infInut.xMotivo
                };
            }
        }
    }

    public enum Status
    {
        SUCESSO,
        ERRO
    }

    public class MensagemRetornoInutilizacao
    {
        public Status Status { get; set; }
        public string Mensagem { get; set; }
        public string Xml { get; set; }
        public string DataInutilizacao { get; set; }
        public string IdInutilizacao { get; set; }
        public string ProtocoloInutilizacao { get; internal set; }
        public string MotivoInutilizacao { get; internal set; }
    }
}
