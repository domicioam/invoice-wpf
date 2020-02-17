using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Utils
{
    public class CstListManager
    {
        public static List<string> GetCstListPorImposto(string nomeImposto)
        {
            switch(nomeImposto)
            {
                case "ICMS":
                    return new List<string>() { "41", "60" };
                case "IPI":
                    return new List<string>() { "40", "50", "60" };
                case "PIS":
                    return new List<string>() { "04", "05", "06", "07", "08", "09" };
                case "COFINS":
                    return new List<string>() { "01", "40", "50", "60" };
                default:
                    return null;
            }
        }
    }
}
