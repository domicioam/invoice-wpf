using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace NFe.Core.Utils.Acentuacao
{
    public static class Acentuacao
    {
        public static string RemoverAcentuacao(string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }
    }
}
