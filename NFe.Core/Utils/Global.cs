using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Utils
{
    public static class Global
    {
        public static readonly string XmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notas Fiscais\XML");
    }
}
