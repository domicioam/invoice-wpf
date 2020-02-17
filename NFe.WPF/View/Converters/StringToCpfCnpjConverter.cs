using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EmissorNFe.View.Converters
{
    public class StringToCpfCnpjConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string valueStr = Regex.Replace((string)value, @"[^0-9]", "");
            string mask = valueStr.Length <= 11 ? "999.999.999-99" : "99.999.999/9999-99";

            for (int i = 0; i < mask.Length; i++)
            {
                if (Char.IsPunctuation(mask[i]))
                {
                    if (i + 1 <= valueStr.Length && !Char.IsPunctuation(valueStr[i]))
                    {
                        valueStr = valueStr.Insert(i, mask[i].ToString());
                    }
                }
            }

            return valueStr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Regex.Replace((string)value, @"[^0-9]", "");
        }
    }
}
