using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MockEngine.Utilities
{
    public static class XmlExtensions
    {
        public static string ToXmlString(this object graph)
        {
            if ( graph == null)
            {
                return "";
            }
            else
            {
                Type type = graph.GetType();
                var serializer = new DataContractSerializer(type);
                var stringWriter = new StringWriter();
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                var xmlWriter = XmlWriter.Create(stringWriter, settings);
                try
                {
                    serializer.WriteObject(xmlWriter, graph);
                    xmlWriter.Flush();
                    return stringWriter.ToString();
                }
                catch( Exception e)
                {
                    return $"<exception>{e.Message}</exception>";
                }
            }
        }
        public static T DeserializeXml<T>(this string xml)
        {
            if (xml == null)
            {
                return default(T);
            }
            else
            {
                var serializer = new DataContractSerializer(typeof(T));
                var stringReader = new StringReader(xml);
                var xmlReader = XmlReader.Create(stringReader);
                return (T)serializer.ReadObject(xmlReader);
            }
        }
    }
}
