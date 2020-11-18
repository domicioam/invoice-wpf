using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;
using NFe.Core.XmlSchemas.NfeInutilizacao2.Envio;
using NFe.Core.Utils.Xml;

namespace NFe.Core.Utils.Assinatura
{
    public static class AssinaturaDigital
    {
        public static XmlDocument AssinarLoteComUmaNota(string xml, string refUri, X509Certificate2 x509Cert, ref string digVal)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            SignedXml signedXml = new SignedXml(doc);
            Reference reference = new Reference();

            signedXml.SigningKey = x509Cert.PrivateKey;

            reference.Uri = refUri;

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
            KeyInfo keyInfo = new KeyInfo();

            reference.AddTransform(env);
            reference.AddTransform(c14);
            signedXml.AddReference(reference);
            keyInfo.AddClause(new KeyInfoX509Data(x509Cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNode firstChild = doc.ChildNodes.Item(1); //revisar código para caso de várias notas dentro do lote
            XmlNode infNfe = firstChild.ChildNodes.Item(2);

            digVal = Convert.ToBase64String(reference.DigestValue);

            infNfe.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc;
        }

        public static XmlDocument AssinarNotaFiscal(string xml, string refUri, X509Certificate2 x509Cert, ref string digVal)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            SignedXml signedXml = new SignedXml(doc);
            Reference reference = new Reference();

            signedXml.SigningKey = x509Cert.PrivateKey;

            reference.Uri = refUri;

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
            KeyInfo keyInfo = new KeyInfo();

            reference.AddTransform(env);
            reference.AddTransform(c14);
            signedXml.AddReference(reference);
            keyInfo.AddClause(new KeyInfoX509Data(x509Cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNode infNfe = doc.ChildNodes.Item(0);


            digVal = Convert.ToBase64String(reference.DigestValue);
            infNfe.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc;
        }

        public static XmlDocument AssinarEvento(string xml, string refUri, X509Certificate2 x509Cert)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            SignedXml signedXml = new SignedXml(doc);
            Reference reference = new Reference();

            signedXml.SigningKey = x509Cert.PrivateKey;

            reference.Uri = refUri;

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
            KeyInfo keyInfo = new KeyInfo();

            reference.AddTransform(env);
            reference.AddTransform(c14);
            signedXml.AddReference(reference);
            keyInfo.AddClause(new KeyInfoX509Data(x509Cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNode firstChild = doc.ChildNodes.Item(1); //revisar código para caso de várias notas dentro do lote
            XmlNode evento = firstChild.ChildNodes.Item(1);

            evento.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc;
        }

        public static XmlDocument AssinarInutilizacao(string xml, string refUri, X509Certificate2 x509Cert)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            SignedXml signedXml = new SignedXml(doc);
            Reference reference = new Reference();

            signedXml.SigningKey = x509Cert.PrivateKey;

            reference.Uri = refUri;

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
            KeyInfo keyInfo = new KeyInfo();

            reference.AddTransform(env);
            reference.AddTransform(c14);
            signedXml.AddReference(reference);
            keyInfo.AddClause(new KeyInfoX509Data(x509Cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNode firstChild = doc.ChildNodes.Item(1); //revisar código para caso de várias notas dentro do lote
            firstChild.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc;
        }

        internal static XmlDocument AssinarConsultaRecibo(string xml, string refUri, X509Certificate2 x509Cert)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            SignedXml signedXml = new SignedXml(doc);
            Reference reference = new Reference();

            signedXml.SigningKey = x509Cert.PrivateKey;

            reference.Uri = refUri;

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
            KeyInfo keyInfo = new KeyInfo();

            reference.AddTransform(env);
            reference.AddTransform(c14);
            signedXml.AddReference(reference);
            keyInfo.AddClause(new KeyInfoX509Data(x509Cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNode firstChild = doc.ChildNodes.Item(1);
            firstChild.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return doc;
        }
    }
}
