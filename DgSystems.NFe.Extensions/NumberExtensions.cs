using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Converts a number formatted string to a double using the culture info and number styles provided.
        /// </summary>
        /// <param name="textNumber"></param>
        /// <param name="culture"></param>
        /// <param name="numberStyles"></param>
        /// <returns>Number parsed from string or 0 if string is null.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static double ToDouble(this string textNumber, CultureInfo culture, NumberStyles numberStyles = NumberStyles.Float)
        {
            if (culture == null)
                throw new ArgumentNullException($"{nameof(culture)} is null.");

            if (textNumber == null)
                return 0;
            
            if (double.TryParse(textNumber, numberStyles, culture, out var number))
            {
                return number;
            }

            throw new ArgumentException($"{textNumber} is not a valid double number.");
        }

        /// <summary>
        /// Converts a number formatted string to a double using the culture info and number styles provided.
        /// </summary>
        /// <param name="textNumber"></param>
        /// <param name="culture"></param>
        /// <param name="numberStyles"></param>
        /// <returns>Number parsed from string or 0 if string is null.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static decimal ToDecimal(this string textNumber, CultureInfo culture, NumberStyles numberStyles = NumberStyles.Float)
        {
            if (culture == null)
                throw new ArgumentNullException($"{nameof(culture)} is null.");

            if (textNumber == null)
                return 0;

            if (decimal.TryParse(textNumber, numberStyles, culture, out var number))
            {
                return number;
            }

            throw new ArgumentException($"{textNumber} is not a valid decimal number.");
        }
    }
}
