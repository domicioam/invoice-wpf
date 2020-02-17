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
        public Service GetService(Modelo modelo, Ambiente ambiente, Servico servico, CodigoUfIbge UF, X509Certificate2 certificado)
        {
            string endpoint = "";

            if (modelo == Modelo.Modelo55)
            {
                if (ambiente == Ambiente.Homologacao)
                {
                    switch (servico)
                    {
                        case Servico.CANCELAMENTO:
                            endpoint = "RecepcaoEventoSoapHomNfe";
                            return GerarServicoCancelamento(UF, certificado, endpoint);
                        case Servico.INUTILIZACAO:
                            endpoint = "NfeInutilizacao2Homologacao";
                            return GerarServicoInutilizacao(UF, certificado, endpoint);
                        case Servico.AUTORIZACAO:
                            endpoint = "NfeAutorizacaoSoapHom";
                            return GerarServicoAutorizacao(UF, certificado, endpoint);
                        case Servico.RetAutorizacao:
                            endpoint = "NfeRetAutorizacaoSoapHom";
                            return GerarServicoRetAutorizacao(UF, certificado, endpoint);
                    }
                }
                else
                {
                    switch (servico)
                    {
                        case Servico.CANCELAMENTO:
                            endpoint = "RecepcaoEventoSoapProdNfe";
                            return GerarServicoCancelamento(UF, certificado, endpoint);
                        case Servico.INUTILIZACAO:
                            endpoint = "NfeInutilizacao2Producao";
                            return GerarServicoInutilizacao(UF, certificado, endpoint);
                        case Servico.AUTORIZACAO:
                            endpoint = "NfeAutorizacaoSoapProd";
                            return GerarServicoAutorizacao(UF, certificado, endpoint);
                        case Servico.RetAutorizacao:
                            endpoint = "NfeRetAutorizacaoSoapProd";
                            return GerarServicoRetAutorizacao(UF, certificado, endpoint);
                    }
                }
            }

            if (modelo == Modelo.Modelo65)
            {
                if (ambiente == Ambiente.Homologacao)
                {
                    switch (servico)
                    {
                        case Servico.CANCELAMENTO:
                            endpoint = "RecepcaoEventoSoapHomNfce";
                            return GerarServicoCancelamento(UF, certificado, endpoint);

                        case Servico.INUTILIZACAO:
                            endpoint = "NfceInutilizacao2Homologacao";
                            return GerarServicoInutilizacao(UF, certificado, endpoint);
                        case Servico.AUTORIZACAO:
                            endpoint = "NfceAutorizacaoSoapHom";
                            return GerarServicoAutorizacao(UF, certificado, endpoint);
                        case Servico.RetAutorizacao:
                            endpoint = "NfceRetAutorizacaoSoapHom";
                            return GerarServicoRetAutorizacao(UF, certificado, endpoint);
                    }
                }
                else
                {
                    switch (servico)
                    {
                        case Servico.CANCELAMENTO:
                            endpoint = "RecepcaoEventoSoapProdNfce";
                            return GerarServicoCancelamento(UF, certificado, endpoint);

                        case Servico.INUTILIZACAO:
                            endpoint = "NfceInutilizacao2Producao";
                            return GerarServicoInutilizacao(UF, certificado, endpoint);
                        case Servico.AUTORIZACAO:
                            endpoint = "NfceAutorizacaoSoapProd";
                            return GerarServicoAutorizacao(UF, certificado, endpoint);
                        case Servico.RetAutorizacao:
                            endpoint = "NfceRetAutorizacaoSoapProd";
                            return GerarServicoRetAutorizacao(UF, certificado, endpoint);
                    }
                }
            }

            return null;
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
