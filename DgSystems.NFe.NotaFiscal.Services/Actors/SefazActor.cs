using Akka.Actor;
using Akka.Util;
using NFe.Core.Domain;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;


namespace DgSystems.NFe.Services.Actors
{
    public class SefazActor : ReceiveActor
    {
        #region Messages
        public class nfeAutorizacaoLote
        {
            public nfeAutorizacaoLote(XmlNode node, NotaFiscal notaFiscal, X509Certificate2 certificado, CodigoUfIbge codigoUf)
            {
                Node = node;
                CodigoUf = codigoUf;
                Certificado = certificado;
                NotaFiscal = notaFiscal;
            }

            public XmlNode Node { get; }
            public CodigoUfIbge CodigoUf { get; }
            public X509Certificate2 Certificado { get; }
            public NotaFiscal NotaFiscal { get; }
        }
        #endregion

        public SefazActor(IServiceFactory serviceFactory)
        {
            Receive<nfeAutorizacaoLote>(HandlenfeAutorizacaoLote);
            ServiceFactory = serviceFactory;
        }

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IServiceFactory ServiceFactory { get; }

        private void HandlenfeAutorizacaoLote(nfeAutorizacaoLote msg)
        {
            log.Info($"{nameof(nfeAutorizacaoLote)} recebida.");

            NFeAutorizacao4Soap client = CriarClientWS(msg.NotaFiscal, msg.Certificado, msg.CodigoUf);
            TProtNFe protocolo = InvocaServico_E_RetornaProtocolo(msg.Node, client);

            Sender.Tell(new Result<TProtNFe>(protocolo));
        }

        private NFeAutorizacao4Soap CriarClientWS(NotaFiscal notaFiscal, X509Certificate2 certificado, CodigoUfIbge codigoUf)
        {
            var servico = ServiceFactory.GetService(notaFiscal.Identificacao.Modelo, Servico.AUTORIZACAO, codigoUf, certificado);
            return (NFeAutorizacao4Soap)servico.SoapClient;
        }

        private static TProtNFe InvocaServico_E_RetornaProtocolo(XmlNode node, NFeAutorizacao4Soap client)
        {
            var inValue = new nfeAutorizacaoLoteRequest { nfeDadosMsg = node };

            var result = client.nfeAutorizacaoLote(inValue).nfeResultMsg;
            var retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);
            return (TProtNFe)retorno.Item;
        }
    }
}
