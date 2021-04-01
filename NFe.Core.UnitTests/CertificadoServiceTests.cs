using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using System;
using Xunit;

namespace NFe.Core.UnitTests
{
    public class CertificadoServiceTests
    {
        //[Fact]
        //public void Should_Return_Certificate_By_Path_When_Path_Exists()
        //{
        //    var certificadoRepositoryMock = new Mock<ICertificadoRepository>();
        //    certificadoRepositoryMock.Setup(c => c.GetCertificado()).Returns(new CertificadoEntity() { Caminho = "path_to_certificate", Senha = "VqkVinLLG4/EAKUokpnVDg==" });
        //    var certificateManagerMock = new Mock<ICertificateManager>();
        //    Mock<RijndaelManagedEncryption> encryptorMock = new Mock<RijndaelManagedEncryption>();
        //    var certificadoService = new CertificadoService(certificadoRepositoryMock.Object, certificateManagerMock.Object, encryptorMock.Object);

        //    certificadoService.GetX509Certificate2(); // how to mock static methods calls

        //    encryptorMock.Verify(e => e.DecryptRijndael(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        //    certificateManagerMock.Verify(c => c.GetCertificateByPath(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        //    certificateManagerMock.Verify(c => c.GetCertificateBySerialNumber(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        //}

        //[Fact]
        //public void Should_Return_Certificate_By_Serial_Number_When_Serial_Number_Exists()
        //{
        //    var certificadoRepositoryMock = new Mock<ICertificadoRepository>();
        //    certificadoRepositoryMock.Setup(c => c.GetCertificado()).Returns(new CertificadoEntity() { Senha = "VqkVinLLG4/EAKUokpnVDg==" });
        //    var certificateManagerMock = new Mock<ICertificateManager>();
        //    Mock<RijndaelManagedEncryption> encryptorMock = new Mock<RijndaelManagedEncryption>();
        //    var certificadoService = new CertificadoService(certificadoRepositoryMock.Object, certificateManagerMock.Object, encryptorMock.Object);

        //    certificadoService.GetX509Certificate2(); // how to mock static methods calls

        //    certificateManagerMock.Verify(c => c.GetCertificateBySerialNumber(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        //    certificateManagerMock.Verify(c => c.GetCertificateByPath(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        //}

        //[Fact]
        //public void Should_Not_Return_Certificate_By_Path_Or_Serial_Number_When_Certificado_Entity_Doesnt_Exists()
        //{
        //    var certificadoRepositoryMock = new Mock<ICertificadoRepository>();
        //    var certificateManagerMock = new Mock<ICertificateManager>();
        //    Mock<RijndaelManagedEncryption> encryptorMock = new Mock<RijndaelManagedEncryption>();
        //    var certificadoService = new CertificadoService(certificadoRepositoryMock.Object, certificateManagerMock.Object, encryptorMock.Object);

        //    var certificate = certificadoService.GetX509Certificate2(); // how to mock static methods calls

        //    Assert.Null(certificate);
        //    certificateManagerMock.Verify(c => c.GetCertificateByPath(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        //    certificateManagerMock.Verify(c => c.GetCertificateBySerialNumber(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        //}
    }
}
