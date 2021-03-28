using System;

namespace NFe.Core.NotaFiscal
{
    public enum RegimeEspecialTributacao
    {
        MicroempresaMunicipal,
        Estimativa,
        SociedadeProfissionais,
        Cooperativa,
        MicroempresarioIndividualMei,
        MicroempresarioEEmpresaPequenoporte
    }

    public class IssqnTotal
    {
        public double TotalServicos { get; set; }
        public double BaseCalculo { get; set; }
        public double TotalIss { get; set; }
        public double Pis { get; set; }
        public double Cofins { get; set; }
        public DateTime DataPrestacaoServico { get; set; }
        public double DeducaoBaseCalculo { get; set; }
        public double Outro { get; set; }
        public double DescontoIncondicionado { get; set; }
        public double DescontoCondicionado { get; set; }
        public double TotalRetencaoIss { get; set; }
        public RegimeEspecialTributacao? RegimeEspecialTributacao { get; set; }
    }
}