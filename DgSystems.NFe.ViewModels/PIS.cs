using System.Collections.Generic;

namespace DgSystems.NFe.ViewModels
{
    public class PIS
    {
        public string CST { get; set; }
        public double Aliquota { get; set; }
        public List<string> CstList { get { return new List<string>() { "04", "05", "06", "07", "08", "09" }; } }
    }
}