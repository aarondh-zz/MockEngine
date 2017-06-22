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
        private const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private const string AttributePropertyPrefix = "_";
        private const string TextPropertyName = "__text";
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
        private void Serialize(XmlWriter writer, IDictionary<object, object> graph)
        {
            if ( graph.Count == 1)
            {
                foreach( var entry in graph )
                {
                    Serialize(writer, entry.Value, entry.Key.ToString(), true);
                }
            }
            else
            {
                Serialize(writer, graph, _defaultRootElementName, true);
            }
        }
        void Serialize(XmlWriter writer, object value, string localName, bool isRoot = false)
        {
            if (value is IList<object>)
            {
                Serialize(writer, value as IList<object>, localName);
            }
            else if (value is IDictionary<object, object>)
            {
                Serialize(writer, value as IDictionary<object, object>, localName);
            }
            else if (localName == TextPropertyName)
            {
                if (value != null)
                {
                    writer.WriteValue(value);
                }

            }
            else if (localName.StartsWith(AttributePropertyPrefix))
            {
                if (value != null)
                {
                    var attributeName = localName.Substring(AttributePropertyPrefix.Length);
                    writer.WriteAttributeString(attributeName, null, value.ToString());
                }

            }
            else
            {
                writer.WriteStartElement(localName);
                if ( isRoot )
                {
                    writer.WriteAttributeString("xmlns", "i", null, "XmlSchemaInstanceNamespace");
                }
                if (value == null)
                {
                    writer.WriteAttributeString("nil", XmlSchemaInstanceNamespace, "true");
                }
                else
                {
                    writer.WriteValue(value);
                }
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
