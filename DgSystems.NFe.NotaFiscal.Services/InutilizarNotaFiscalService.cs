using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NFeInutilizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores;
using System;
using System.Xml;
using Envio = NFe.Core.XmlSchemas.NfeInutilizacao2.Envio;
using Retorno = NFe.Core.XmlSchemas.NfeInutilizacao2.Retorno;
using Status = NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2.Status;

namespace NFe.Core.Sefaz.Facades
{
    public class InutilizarNotaFiscalService : IInutilizarNotaFiscalService
    {
        private readonly INotaInutilizadaRepository _notaInutilizadaService;
        private readonly SefazSettings _sefazSettings;
        private readonly CertificadoService _certificadoService;
        private readonly IServiceFactory _serviceFactory;

        public InutilizarNotaFiscalService(INotaInutilizadaRepository notaInutilizadaService, SefazSettings sefazSettings, CertificadoService certificadoService, IServiceFactory serviceFactory)
        {
            _notaInutilizadaService = notaInutilizadaService;
            _sefazSettings = sefazSettings;
            _certificadoService = certificadoService;
            _serviceFactory = serviceFactory;
        }

        protected InutilizarNotaFiscalService()
        {

        }

        public virtual RetornoInutilizacao InutilizarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, Modelo modeloNota,
            string serie, string numeroInicial, string numeroFinal)
        {
            RetornoInutilizacao retorno = InutilizarNotaFiscal(codigoUf, _sefazSettings.Ambiente, cnpjEmitente, modeloNota, serie, numeroInicial, numeroFinal);

            if (retorno.Status == Status.ERRO)
                return retorno;
            
            var notaInutilizada = new NotaInutilizada
            {
                DataInutilizacao = DateTime.Now,
                IdInutilizacao = retorno.IdInutilizacao,
                Modelo = modeloNota == Modelo.Modelo55 ? 55 : 65,
                Motivo = retorno.MotivoInutilizacao,
                Numero = numeroInicial,
                Protocolo = retorno.ProtocoloInutilizacao,
                Serie = serie
            };

            _notaInutilizadaService.Salvar(notaInutilizada, retorno.Xml);

            return retorno;
        }

        private RetornoInutilizacao InutilizarNotaFiscal(CodigoUfIbge codigoUf, Ambiente ambiente, string cnpjEmitente, Modelo modeloNota, string serie,
            string numeroInicial, string numeroFinal)
        {
            var infInut = new Envio.TInutNFeInfInut
            {
                tpAmb = (Envio.TAmb)(int)ambiente,
                xServ = Envio.TInutNFeInfInutXServ.INUTILIZAR,
                cUF = (Envio.TCodUfIBGE)(int)codigoUf.ToTCOrgaoIBGE(),
                ano = DateTime.Now.ToString("yy"),
                CNPJ = cnpjEmitente,
                mod = (Envio.TMod)(int)modeloNota,
                serie = serie,
                nNFIni = numeroInicial,
                nNFFin = numeroFinal,
                xJust = "Não usada, quebra de sequência."
            };

            var cUF = infInut.cUF.ToString().Replace("Item", string.Empty);
            var modelo = modeloNota.ToString().Replace("Modelo", string.Empty);

            infInut.Id = $"ID{cUF}{infInut.ano}{cnpjEmitente}{modelo}{int.Parse(serie):D3}{int.Parse(numeroInicial):D9}{int.Parse(numeroFinal):D9}";

            var inutNFe = new Envio.TInutNFe { versao = "4.00", infInut = infInut };
            var xml = XmlUtil.Serialize(inutNFe, "http://www.portalfiscal.inf.br/nfe");
            var certificado = _certificadoService.GetX509Certificate2();
            XmlDocument node = AssinaturaDigital.AssinarInutilizacao(xml, "#" + infInut.Id, certificado);

            var servico = _serviceFactory.GetService(modeloNota, Servico.INUTILIZACAO, codigoUf, certificado);
            var client = servico.SoapClient as NFeInutilizacao4SoapClient;
            var result = client.nfeInutilizacaoNF(node);
            var retorno = XmlUtil.Deserialize<Retorno.TRetInutNFe>(result.OuterXml) as Retorno.TRetInutNFe;

            if (!retorno.infInut.cStat.Equals("102"))
                return new RetornoInutilizacao { Status = Status.ERRO, Mensagem = retorno.infInut.xMotivo };
            
            var procSerialized = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ProcInutNFe versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\">"
                + node.OuterXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                + result.OuterXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                + "</ProcInutNFe>";

            return new RetornoInutilizacao
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
    }
}
