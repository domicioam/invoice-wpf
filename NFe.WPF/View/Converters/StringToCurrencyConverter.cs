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
    public class StringToCurrencyConverter : IValueConverter
    {
        private CultureInfo cultureInfo;

        public StringToCurrencyConverter()
        {
            cultureInfo = new CultureInfo("pt-BR");
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString("N2", cultureInfo);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueStr = Regex.Replace((string)value, "[\\.,]+", "");

            if (!IsDigitsOnly(valueStr))
            {
                valueStr = Regex.Replace(valueStr, "[^0-9]+", "");
            }

            valueStr = valueStr.Insert(valueStr.Length - 2, ".");
            return double.Parse(valueStr, CultureInfo.InvariantCulture);
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
