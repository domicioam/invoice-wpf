using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.NotasFiscais;

namespace NFe.Core.Sefaz.Facades
{
    class QrCode
    {
        private string _qrCode;

        public QrCode()
        {
            _qrCode = string.Empty;
        }

        public void GerarQrCodeNFe(string chave, Destinatario destinatario, string digestValue, Ambiente ambiente, DateTime dhEmissao,
            string valorNF, string valorICMS, string cIdToken, string csc, TipoEmissao tipoEmissão)
        {
            if (tipoEmissão == TipoEmissao.Normal)
            {
                _qrCode = GerarQrCodeNFeEmissãoOnline(chave, ambiente, cIdToken, csc);
            }
            else
            {
                _qrCode = GerarQrCodeNFeEmissãoOffline(chave, digestValue, ambiente, dhEmissao.Day, valorNF, cIdToken, csc);
            }
        }

        private string GerarQrCodeNFeEmissãoOnline(string chave, Ambiente ambiente, string cscId, string csc)
        {
            string versãoQrCode = "2";
            int ambienteNum = (int)ambiente + 1;

            string entradaSHA1 = $"{chave}|{versãoQrCode}|{(int)ambiente + 1}|{Int32.Parse(cscId)}";

            string saidaSHA1;

            using (var sha1 = new SHA1Managed())
            {
                saidaSHA1 = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(entradaSHA1 + csc))).Replace("-", "");
            }

            //return "http://www.fazenda.df.gov.br/nfce/qrcode?p=" + $"{entradaSHA1}|{saidaSHA1}";
            return "http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx?p=" + $"{entradaSHA1}|{saidaSHA1}";
        }

        private string GerarQrCodeNFeEmissãoOffline(string chave, string digestValue, Ambiente ambiente, int diaEmissão, string valorNF, string cIdToken, string csc)
        {
            string versãoQrCode = "2";
            var digValHex = BitConverter.ToString(Encoding.UTF8.GetBytes(digestValue)).Replace("-", "").ToLower();

            string entradaSHA1 = $"{chave}|{versãoQrCode}|{(int)ambiente + 1}|{diaEmissão.ToString().PadLeft(2, '0')}|{valorNF}|{digValHex}|{Int32.Parse(cIdToken)}";

            string saidaSHA1;

            using (var sha1 = new SHA1Managed())
            {
                saidaSHA1 = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(entradaSHA1 + csc))).Replace("-", "");
            }

            //return "http://www.fazenda.df.gov.br/nfce/qrcode?p=" + $"{entradaSHA1}|{saidaSHA1}";
            return "http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx?p=" + $"{entradaSHA1}|{saidaSHA1}";
        }

        public override string ToString()
        {
            return _qrCode;
        }
    }
}
