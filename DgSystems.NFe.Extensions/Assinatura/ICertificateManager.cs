using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace NFe.Core.Utils.Assinatura
{
    public interface ICertificateManager
    {
        X509Certificate2 GetCertificateByName(string certificateName, bool usaWService);
        X509Certificate2 GetCertificateByPath(string caminho, string senha);
        X509Certificate2 GetCertificateBySerialNumber(string serialNumber, bool usaWService);
        Certificate GetFriendlyCertificate(string caminho, string senha);
        List<Certificate> GetFriendlyCertificates();
    }
}