using MockEngine.Http.Interfaces;
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
    public class XmlDictionarySerializer : ISerializer
    {
        private string _rootElementName;
        public XmlDictionarySerializer ( string rootElementName )
        {
            _rootElementName = rootElementName;
        }
        public void Serialize(Stream stream, object graph )
        {
            if ( graph is IDictionary<object,object>)
            {
                var writer = new XmlTextWriter(stream, Encoding.UTF8);
                Serialize(writer, graph as IDictionary<object, object>);
                writer.Flush();
            }
        }
        void Serialize(XmlWriter writer, IDictionary<object, object> graph)
        {
            Serialize(writer, graph, _rootElementName);
        }
        void Serialize(XmlWriter writer, IDictionary<object, object> graph, string localName)
        {
            writer.WriteStartElement(localName);
            if (graph != null)
            {
                foreach (var entry in graph)
                {
                    var childName = entry.Key.ToString();
                    if (entry.Value is IList<object>)
                    {
                        Serialize(writer, entry.Value as IList<object>, childName);
                    }
                    else if (entry.Value is IDictionary<object, object>)
                    {
                        Serialize(writer, entry.Value as IDictionary<object, object>, childName);
                    }
                    else
                    {
                        writer.WriteValue(entry.Value);
                    }
                }
            }
            writer.WriteEndElement();
        }
        void Serialize(XmlWriter writer, IList<object> graph, string localName)
        {
            if (graph != null)
            {
                foreach (var entry in graph)
                {
                    writer.WriteStartElement(localName);
                    if (entry is IList<object>)
                    {
                        Serialize(writer, entry as IList<object>, localName);
                    }
                    else if (entry is IDictionary<object, object>)
                    {
                        Serialize(writer, entry as IDictionary<object, object>, localName);
                    }
                    else
                    {
                        writer.WriteValue(entry);
                    }
                    writer.WriteEndElement();
                }
            }
        }
    }
}
