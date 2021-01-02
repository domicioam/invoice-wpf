using System.Collections.Generic;

namespace DgSystems.NFe.ViewModels
{
    public class COFINS
    {
        public string CST { get; set; }
        public double Aliquota { get; set; }
        public List<string> CstList { get { return new List<string>() { "01", "40", "50", "60" }; } }
    }
}