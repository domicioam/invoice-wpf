using System;

namespace NFe.Core.NotasFiscais.Entities
{
    public enum TipoDestinatario
    {
        PessoaFisica,
        PessoaJuridica,
        Estrangeiro
    }

    public class Destinatario
    {
        public Destinatario(string nomeRazao)
        {
            NomeRazao = nomeRazao;
        }

        public Destinatario(Ambiente ambiente, Modelo modelo, string telefone, string email, Endereco endereco,
            TipoDestinatario tipoDestinatario, string inscricaoEstadual = null, bool isIsentoICMS = false,
            string documento = null, string nomeRazao = null)
        {
            Documento = documento;
            NomeRazao = ambiente == Ambiente.Homologacao
                ? "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
                : nomeRazao ?? "CONSUMIDOR NÃO IDENTIFICADO";
            IsIsentoICMS = isIsentoICMS;
            TipoDestinatario = tipoDestinatario;

            if (Documento == null) 
                return;

            Email = email;

            if (modelo == Modelo.Modelo65)
            {
                IndicadorInscricaoEstadual = 9;
                InscricaoEstadual = null;
            }
            else
            {
                IndicadorInscricaoEstadual = IsIsentoICMS ? 2 : 1;

                InscricaoEstadual = inscricaoEstadual;
            }

            Telefone = telefone;
            Endereco = endereco;
        }

        public string Documento { get; set; }
        public string NomeRazao { get; set; }
        public Endereco Endereco { get; set; }
        public string Telefone { get; set; }
        public int IndicadorInscricaoEstadual { get; set; }
        public string InscricaoEstadual { get; set; }
        public string Email { get; set; }
        public TipoDestinatario TipoDestinatario { get; set; }

        public string DocumentoDanfe
        {
            get
            {
                if (string.IsNullOrEmpty(Documento))
                    return null;

                string tipoDocumento;
                string mask;

                switch (TipoDestinatario)
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

                var cpfCnpjMasked = Documento;

                for (var i = 0; i < mask.Length; i++)
                    if (char.IsPunctuation(mask[i]))
                        if (i + 1 <= cpfCnpjMasked.Length && !char.IsPunctuation(cpfCnpjMasked[i]))
                            cpfCnpjMasked = cpfCnpjMasked.Insert(i, mask[i].ToString());

                return tipoDocumento + cpfCnpjMasked;
            }
        }

        public bool IsIsentoICMS { get; set; }
    }
}