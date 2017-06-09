using MockEngine.Http.Interfaces;
using MockEngine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MockEngine.Http.Utilities
{
    public class DynamicXmlSerializer : ISerializer
    {
        private Encoding _encoding;
        public DynamicXmlSerializer( Encoding encoding = null)
        {
            if ( encoding == null)
            {
                _encoding = Encoding.UTF8;
            }
            else
            {
                _encoding = encoding;
            }
        }
        public void Serialize(Stream stream, object graph)
        {
            if ( graph is DynamicXml)
            {
                var dynamicXml = graph as DynamicXml;
                var bytes = _encoding.GetBytes(dynamicXml.ToString());
                stream.Write(bytes, 0, bytes.Length );
            }
        }
    }
}
