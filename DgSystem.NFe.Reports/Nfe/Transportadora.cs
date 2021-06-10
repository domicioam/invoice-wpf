using System;

namespace DgSystem.NFe.Reports.Nfe
{
    [Serializable]
    public class Transportadora
    {
        public Transportadora(string nome, string modalidadeFrete, string codigoANT, string veiculoPlaca, string veiculoUF, string cpfCnpj, string enderecoCompleto, string municipio, string uF, string inscricaoEstadual, string siglaUF)
        {
            Nome = nome;
            ModalidadeFrete = modalidadeFrete;
            CodigoANT = codigoANT;
            VeiculoPlaca = veiculoPlaca;
            VeiculoUF = veiculoUF;
            CpfCnpj = cpfCnpj;
            EnderecoCompleto = enderecoCompleto;
            Municipio = municipio;
            UF = uF;
            InscricaoEstadual = inscricaoEstadual;
            SiglaUF = siglaUF;
        }

        public string Nome { get; }
        public string ModalidadeFrete { get; }
        public string CodigoANT { get; }
        public string VeiculoPlaca { get; }
        public string VeiculoUF { get; }
        public string CpfCnpj { get; }
        public string EnderecoCompleto { get; }
        public string Municipio { get; }
        public string UF { get; }
        public string InscricaoEstadual { get; }
        public string SiglaUF { get; }
    }
}