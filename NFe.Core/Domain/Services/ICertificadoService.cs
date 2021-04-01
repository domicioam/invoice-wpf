using NFe.Core.Utils.Assinatura;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace NFe.Core.Cadastro.Certificado
{
    public interface ICertificadoService
    {
        X509Certificate2 GetX509Certificate2();
        List<Certificate> GetFriendlyCertificates();
        //X509Certificate2 GetCertificateByName(string certificateName, bool usaWService);
        //X509Certificate2 GetCertificateByPath(string caminho, string senha);
        //X509Certificate2 GetCertificateBySerialNumber(string serialNumber, bool usaWService);
        Certificate GetFriendlyCertificate(string caminho, string senha);
    }
}