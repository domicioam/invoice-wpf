using NFe.Core.Domain;
using System.Security.Cryptography.X509Certificates;

namespace NFe.Core.NotasFiscais
{
    public interface IServiceFactory
    {
        Service GetService(Modelo modelo, Servico servico, CodigoUfIbge UF, X509Certificate2 certificado);
    }
}