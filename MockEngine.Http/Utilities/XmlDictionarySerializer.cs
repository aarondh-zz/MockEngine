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
        private string _defaultRootElementName;
        public XmlDictionarySerializer ( string defaultRootElementName)
        {
            _defaultRootElementName = defaultRootElementName;
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
            if ( graph.Count == 1)
            {
                foreach( var entry in graph )
                {
                    Serialize(writer, entry.Value, entry.Key.ToString());
                }
            }
            else
            {
                Serialize(writer, graph, _defaultRootElementName);
            }
        }
        void Serialize(XmlWriter writer, object value, string localName)
        {
            if (value is IList<object>)
            {
                Serialize(writer, value as IList<object>, localName);
            }
            else if (value is IDictionary<object, object>)
            {
                Serialize(writer, value as IDictionary<object, object>, localName);
            }
            else
            {
                writer.WriteStartElement(localName);
                writer.WriteValue(value);
                writer.WriteEndElement();
            }
        }
        void Serialize(XmlWriter writer, IDictionary<object, object> graph, string localName)
        {
            writer.WriteStartElement(localName);
            if (graph != null)
            {
                foreach (var entry in graph)
                {
                    var childName = entry.Key.ToString();
                    Serialize(writer, entry.Value, childName);
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
