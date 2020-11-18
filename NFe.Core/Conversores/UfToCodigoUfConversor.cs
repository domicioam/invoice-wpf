using System;
using System.Collections.Generic;
using System.Text;

namespace NFe.Core.Utils.Conversores
{
    public class UfToCodigoUfConversor
    {
        public static string GetCodigoUf(string uf)
        {
            switch (uf)
            {
                case "DF":
                    return "53";

                default:
                    throw new ArgumentException("Argumento inválido.");
            }
        }
    }
}
