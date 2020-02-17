using NFe.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Utils.Xml
{
    public static class XmlExtensions
    {
        public static string LoadXml(this IXmlFileWritable xmlFileWritable)
        {
            var path = Path.Combine(Global.XmlPath, xmlFileWritable.FileName);

            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        public static Task<string> LoadXmlAsync(this IXmlFileWritable xmlFileWritable)
        {
            return Task.Run(() =>
            {
                var path = Path.Combine(Global.XmlPath, xmlFileWritable.FileName);

                return File.Exists(path) ? File.ReadAllText(path) : null;
            });
        }
    }
}
