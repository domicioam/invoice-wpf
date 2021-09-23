using NFe.Core.Interfaces;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace NFe.Core.Cadastro.Certificado
{
    public class CertificadoService
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICertificadoRepository certificadoRepository;
        private readonly RijndaelManagedEncryption encryptor;

        protected CertificadoService() { }

        public CertificadoService(ICertificadoRepository certificadoRepository, RijndaelManagedEncryption encryptor)
        {
            this.certificadoRepository = certificadoRepository;
            this.encryptor = encryptor;
        }

        public virtual X509Certificate2 GetX509Certificate2()
        {
            X509Certificate2 certificado;
            var certificadoEntity = certificadoRepository.GetCertificado();

            if (certificadoEntity == null) return null;

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = GetCertificateByPath(certificadoEntity.Caminho,
                    encryptor.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            return certificado;
        }

        private X509Certificate2 GetCertificateByPath(string caminho, string senha)
        {
            return new X509Certificate2(@caminho, senha);
        }

        public X509Certificate2 GetCertificateBySerialNumber(string serialNumber, bool usaWService)
        {
            try
            {
                X509Certificate2Collection certCollection = GetCertificateCollection(usaWService).Find(X509FindType.FindBySerialNumber, serialNumber, false);
                X509Certificate2 _X509Cert = new X509Certificate2();
                if (certCollection.Count == 0)
                    _X509Cert = null;
                else
                    _X509Cert = certCollection[0];

                return _X509Cert;
            }
            catch (Exception e)
            {
                log.Error(e);
                return null;
            }
        }

        private X509Certificate2Collection GetCertificateCollection(bool usaWService)
        {
            StoreLocation storeLocation = StoreLocation.CurrentUser;

            if (usaWService)
                storeLocation = StoreLocation.LocalMachine;

            X509Store store = new X509Store("MY", storeLocation);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection2 = 
                store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, true);

            store.Close();
            return collection2;
        }

        public X509Certificate2 GetCertificateByName(string certificateName, bool usaWService)
        {
            try
            {
                X509Certificate2Collection scollection = GetCertificateCollection(usaWService).Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);
                X509Certificate2 _X509Cert = new X509Certificate2();
                if (scollection.Count == 0)
                    _X509Cert = null;
                else
                    _X509Cert = scollection[0];

                return _X509Cert;
            }
            catch (Exception e)
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
}