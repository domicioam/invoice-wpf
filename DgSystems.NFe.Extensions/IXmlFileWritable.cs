using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Interfaces
{
    public interface IXmlFileWritable
    {
        string FileName { get; }
        string XmlPath { get; set; }
    }
}
