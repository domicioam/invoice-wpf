using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Utils.QrCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.UnitTests
{
    [TestClass]
    public class QrCodeUtilTest
    {
        [TestMethod]
        public void GerarQrCodeNFeEmissãoOnline_ShouldReturnCorrectQrCode()
        {
            string qrCode = QrCodeUtil.GerarQrCodeNFe("53180904585789000140650030000002561000002566", null, "hxbsVs7OyDdBqCepI6VlkUfkspM=", Ambiente.Homologacao, DateTime.ParseExact("2018-09-30T08:56:34-03:00", "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture), "65.00", "0.00", "000001", "D32FCA9A-D485-489E-9001-78BCEFF8B198", TipoEmissao.Normal);

            Assert.AreEqual("http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx?p=53180904585789000140650030000002561000002566|2|2|1|C1A66513D0CCA30AEA8F5168A8A80B4787530272", qrCode);
        }

        [TestMethod]
        public void GerarQrCodeNFeEmissãoOffline_ShouldReturnCorrectQrCode()
        {
            string qrCode = QrCodeUtil.GerarQrCodeNFe("53180904585789000140650030000002561000002566", null, "hxbsVs7OyDdBqCepI6VlkUfkspM=", Ambiente.Homologacao, DateTime.ParseExact("2018-09-30T08:56:34-03:00", "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture), "65.00", "0.00", "000001", "D32FCA9A-D485-489E-9001-78BCEFF8B198", TipoEmissao.ContigenciaNfce);

            Assert.AreEqual("http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx?p=53180904585789000140650030000002561000002566|2|2|30|65.00|687862735673374f79446442714365704936566c6b55666b73704d3d|1|D1FF4BAE6B32811BE4D534556E4516CC4A421588", qrCode);
        }
    }
}
