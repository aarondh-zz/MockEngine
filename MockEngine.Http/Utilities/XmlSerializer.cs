using MockEngine.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Http.Utilities
{
    public class XmlSerializer : System.Xml.Serialization.XmlSerializer, ISerializer
    {
        public XmlSerializer( Type type) :base(type) { }
    }
}
