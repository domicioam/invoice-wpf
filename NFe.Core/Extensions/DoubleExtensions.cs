using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Extensions
{
    static class DoubleExtensions
    {
        public static string AsNumberFormattedString(this double value)
        {
            return value.ToString("F", CultureInfo.InvariantCulture);
        }
    }
}
