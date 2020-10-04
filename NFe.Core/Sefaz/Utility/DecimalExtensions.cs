using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Sefaz.Utility
{
    static class DecimalExtensions
    {
        public static string ToPositiveDecimalAsStringOrNull(this decimal value)
        {
            if(value < 0)
            {
                return value.ToString();
            }

            return null;
        }
    }
}
