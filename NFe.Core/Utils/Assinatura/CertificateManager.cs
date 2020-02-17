using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace NFe.Core.Utils.Assinatura
{
    public class CertificateManager : ICertificateManager
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CertificateManager()
        {
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) =>
            {
                return c.Subject.Contains("ARSERPRO") || c.Subject.Contains("outlook.com");
            };
        }

        public X509Certificate2 GetCertificateByPath(string caminho, string senha)
        {
            return new X509Certificate2(@caminho, senha);
        }

        public X509Certificate2 GetCertificateBySerialNumber(string serialNumber, bool usaWService)
        {
            X509Certificate2 _X509Cert = new X509Certificate2();

            try
            {
                X509Certificate2Collection collection2 = GetCertificateCollection(usaWService);

                X509Certificate2Collection scollection = collection2.Find(X509FindType.FindBySerialNumber, serialNumber, false);
                if (scollection.Count == 0)
                {
                    _X509Cert = null;
                }
                else
                {
                    _X509Cert = scollection[0];
                }

                return _X509Cert;
            }
            catch(Exception e)
            {
                log.Error(e);
                return null;
            }
        }

        private X509Certificate2Collection GetCertificateCollection(bool usaWService)
        {
            StoreLocation StLocation = StoreLocation.CurrentUser;

            if (usaWService)
                StLocation = StoreLocation.LocalMachine;

            X509Store store = new X509Store("MY", StLocation);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = store.Certificates;
            X509Certificate2Collection collection1 = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, true);
            X509Certificate2Collection collection2 = collection1.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, true);

            store.Close();

            return collection2;
        }

        public X509Certificate2 GetCertificateByName(string certificateName, bool usaWService)
        {
            X509Certificate2 _X509Cert = new X509Certificate2();

            try
            {
                X509Certificate2Collection collection2 = GetCertificateCollection(usaWService);

                X509Certificate2Collection scollection = collection2.Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);
                if (scollection.Count == 0)
                {
                    _X509Cert = null;
                }
                else
                {
                    _X509Cert = scollection[0];
                }

                return _X509Cert;
            }
            catch(Exception e)
            {
                log.Error(e);
                return null;
            }
        }

        public Certificate GetFriendlyCertificate(string caminho, string senha)
        {
            var friendlyCertificate = new Certificate();
            var certificate = GetCertificateByPath(caminho, senha);

            Regex regex = new Regex("CN=(.*?),");
            var matchIssuer = regex.Match(certificate.Issuer);
            string resultIssuer = matchIssuer.Groups[1].ToString();

            friendlyCertificate.FriendlyIssuer = resultIssuer;

            var matchSubject = regex.Match(certificate.Subject);
            var resultSubject = matchSubject.Groups[1].ToString();

            friendlyCertificate.FriendlySubjectName = resultSubject;
            friendlyCertificate.SerialNumber = certificate.SerialNumber;


            return friendlyCertificate;
        }

        public List<Certificate> GetFriendlyCertificates()
        {
            var certificateCollection = GetCertificateCollection(false);
            var friendlyCertificateCollection = new List<Certificate>();

            foreach (var certificate in certificateCollection)
            {
                var friendlyCertificate = new Certificate();

                Regex regex = new Regex("CN=(.*?),");
                var matchIssuer = regex.Match(certificate.Issuer);
                string resultIssuer = matchIssuer.Groups[1].ToString();

                friendlyCertificate.FriendlyIssuer = resultIssuer;

                var matchSubject = regex.Match(certificate.Subject);
                var resultSubject = matchSubject.Groups[1].ToString();

                friendlyCertificate.FriendlySubjectName = resultSubject;
                friendlyCertificate.SerialNumber = certificate.SerialNumber;

                if (!string.IsNullOrEmpty(friendlyCertificate.FriendlySubjectName))
                {
                    friendlyCertificateCollection.Add(friendlyCertificate);
                }
            }

            return friendlyCertificateCollection;
        }
    }

    public class Certificate
    {
        public string FriendlySubjectName { get; set; }
        public string FriendlyIssuer { get; set; }
        public string SerialNumber { get; set; }

        public override string ToString()
        {
            return FriendlySubjectName + " - " + FriendlyIssuer;
        }
    }
}
