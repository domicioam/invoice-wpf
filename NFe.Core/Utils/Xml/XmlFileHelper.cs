using NFe.Interfaces;
using System.IO;

namespace NFe.Core.Utils.Xml
{
    public static class XmlFileHelper
    {
        public static string SaveXmlFile(IXmlFileWritable fileWritable, string xml)
        {
            var path = Path.Combine(Global.XmlPath, fileWritable.FileName);

            if (!Directory.Exists(Global.XmlPath))
            {
                Directory.CreateDirectory(Global.XmlPath);
            }

            using (var stream = File.Create(path))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(xml);
                }
            }

            return path;
        }

        public static bool DeleteXmlFile(IXmlFileWritable fileWritable)
        {
            var path = Path.Combine(Global.XmlPath, fileWritable.FileName);

            if (!File.Exists(path)) return false;

            File.Delete(path);
            return true;

        }
    }
}
