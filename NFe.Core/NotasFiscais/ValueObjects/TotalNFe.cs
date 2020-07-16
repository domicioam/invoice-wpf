namespace NFe.Core.NotasFiscais.ValueObjects
{
    public class TotalNFe
    {
        public IcmsTotal IcmsTotal { get; set; }
        public IssqnTotal IssqnTotal { get; set; }
        public RetencaoTributosFederais RetencaoTributosFederais { get; set; }
    }
}