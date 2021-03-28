
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotaFiscal
{
    public class Documento
    {
        public string Numero { get; }

        public Documento(string documento)
        {
            Numero = documento;
        }

        public string GetDocumentoDanfe(TipoDestinatario tipoDestinatario)
        {
            if (string.IsNullOrEmpty(Numero))
                return null;

            string tipoDocumento;
            string mask;

            switch (tipoDestinatario)
            {
                case TipoDestinatario.PessoaFisica:
                    tipoDocumento = "CPF: ";
                    mask = "999.999.999-99";
                    break;

                case TipoDestinatario.PessoaJuridica:
                    tipoDocumento = "CNPJ: ";
                    mask = "99.999.999/9999-99";
                    break;

                case TipoDestinatario.Estrangeiro:
                    tipoDocumento = "ID Estrangeiro: ";
                    mask = string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var cpfCnpjMasked = Numero;

            for (var i = 0; i < mask.Length; i++)
                if (char.IsPunctuation(mask[i]))
                    if (i + 1 <= cpfCnpjMasked.Length && !char.IsPunctuation(cpfCnpjMasked[i]))
                        cpfCnpjMasked = cpfCnpjMasked.Insert(i, mask[i].ToString());

            return tipoDocumento + cpfCnpjMasked;

        }
    }
}
