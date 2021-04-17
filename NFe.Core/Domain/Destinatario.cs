using NFe.Core.Domain;
using System;

namespace NFe.Core.Domain
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
            Documento documento = null, string nomeRazao = null)
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

        public Documento Documento { get; set; }
        public string NomeRazao { get; set; }
        public Endereco Endereco { get; set; }
        public string Telefone { get; set; }
        public int IndicadorInscricaoEstadual { get; set; }
        public string InscricaoEstadual { get; set; }
        public string Email { get; set; }
        public TipoDestinatario TipoDestinatario { get; set; }
        public bool IsIsentoICMS { get; set; }

        public static Destinatario CreateDestinatario(string logradouro, string inscricaoEstadual, string nomeRazao, string telefone, 
            string email, string documentoInput, string numero, string bairro, string municipio, string cep, string uf, bool isEstrangeiro, Ambiente ambiente, Modelo _modelo)
        {
            if (string.IsNullOrEmpty(documentoInput)) return null;

            Endereco endereco = null;

            if (documentoInput != null)
            {
                if (logradouro != null)
                {
                    endereco = new Endereco(logradouro, numero, bairro,
                        municipio, cep, uf);
                }
            }
            else
            {
                nomeRazao = "CONSUMIDOR NÃO IDENTIFICADO";
            }

            TipoDestinatario tipoDestinatario;

            if (isEstrangeiro)
                tipoDestinatario = TipoDestinatario.Estrangeiro;
            else if (documentoInput.Length == 11)
                tipoDestinatario = TipoDestinatario.PessoaFisica;
            else
                tipoDestinatario = TipoDestinatario.PessoaJuridica;

            return new Destinatario(ambiente, _modelo, telefone,
                email, endereco, tipoDestinatario, inscricaoEstadual, documento: new Documento(documentoInput),
                nomeRazao: nomeRazao);
        }

    }
}