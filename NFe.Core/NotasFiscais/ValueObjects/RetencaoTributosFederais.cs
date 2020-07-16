namespace NFe.Core.NotasFiscais.ValueObjects
{
    public class RetencaoTributosFederais
    {
        public double TotalRetidoPis { get; set; }
        public double TotalRetidoCofins { get; set; }
        public double TotalRetidoCsll { get; set; }
        public double BaseCalculoIrrf { get; set; }
        public double TotalRetidoIrrf { get; set; }
        public double BaseCalculoRetencaoPrevidenciaSocial { get; set; }
        public double TotalRetencaoPrevidenciaSocial { get; set; }
    }
}