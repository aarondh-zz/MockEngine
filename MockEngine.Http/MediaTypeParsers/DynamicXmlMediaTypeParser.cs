using MockEngine.Http.Interfaces;
using MockEngine.Http.Utilities;
using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Http.MediaTypeParsers
{
    public class DynamicXmlMediaTypeParser : IMediaTypeParser
    {
        public void Initialize(IMockContext context)
        {
        }

        public object Parse(string media, Type type)
        {
            return DynamicXml.Parse(media);
        }
    }
}
