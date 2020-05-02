using System;
using System.Security.Cryptography.X509Certificates;
using Evento = NFe.Core.NFeRecepcaoEvento4;

namespace NFe.Core.NotasFiscais
{
    public enum Servico
    {
        CONSULTA,
        ENVIO,
        CANCELAMENTO,
        STATUS,
        INUTILIZACAO,
        AUTORIZACAO,
        RetAutorizacao
    }

    public class ServiceFactory : IServiceFactory
    {
        public Service GetService(Modelo modelo, Servico servico, CodigoUfIbge UF, X509Certificate2 certificado)
        {
            string endpoint;
            if (modelo == Modelo.Modelo55)
            {
                switch (servico)
                {
                    case Servico.CANCELAMENTO:
                        endpoint = "RecepcaoEventoSoapNfe";
                        return GerarServicoCancelamento(UF, certificado, endpoint);
                    case Servico.INUTILIZACAO:
                        endpoint = "NfeInutilizacao2";
                        return GerarServicoInutilizacao(UF, certificado, endpoint);
                    case Servico.AUTORIZACAO:
                        endpoint = "NfeAutorizacaoSoap";
                        return GerarServicoAutorizacao(UF, certificado, endpoint);
                    case Servico.RetAutorizacao:
                        endpoint = "NfeRetAutorizacaoSoap";
                        return GerarServicoRetAutorizacao(UF, certificado, endpoint);
                }
            }

            if (modelo == Modelo.Modelo65)
            {
                switch (servico)
                {
                    case Servico.CANCELAMENTO:
                        endpoint = "RecepcaoEventoSoapNfce";
                        return GerarServicoCancelamento(UF, certificado, endpoint);

                    case Servico.INUTILIZACAO:
                        endpoint = "NfceInutilizacao2";
                        return GerarServicoInutilizacao(UF, certificado, endpoint);
                    case Servico.AUTORIZACAO:
                        endpoint = "NfceAutorizacaoSoap";
                        return GerarServicoAutorizacao(UF, certificado, endpoint);
                    case Servico.RetAutorizacao:
                        endpoint = "NfceRetAutorizacaoSoap";
                        return GerarServicoRetAutorizacao(UF, certificado, endpoint);
                }
            }

            throw new ArgumentException("Configuration not found!");
        }

        private static Service GerarServicoAutorizacao(CodigoUfIbge UF, X509Certificate2 certificado, string endpoint)
        {
            var soapClient = new NFeAutorizacao4.NFeAutorizacao4SoapClient(endpoint);
            soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;

            return new Service() { SoapClient = soapClient };
        }

        private static Service GerarServicoRetAutorizacao(CodigoUfIbge UF, X509Certificate2 certificado, string endpoint)
        {
            var soapClient = new NFeRetAutorizacao4.NFeRetAutorizacao4SoapClient(endpoint);
            soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;

            return new Service() { SoapClient = soapClient };
        }

        private static Service GerarServicoInutilizacao(CodigoUfIbge UF, X509Certificate2 certificado, string endpoint)
        {
            var soapClient = new NFeInutilizacao4.NFeInutilizacao4SoapClient(endpoint);
            soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;

            return new Service() { SoapClient = soapClient };
        }

        private static Service GerarServicoCancelamento(CodigoUfIbge UF, X509Certificate2 certificado, string endpoint)
        {
            var soapClient = new Evento.NFeRecepcaoEvento4SoapClient(endpoint);
            soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;

            return new Service() { SoapClient = soapClient };
        }
    }

    public class Service
    {
        public object SoapClient { get; set; }
    }
}
