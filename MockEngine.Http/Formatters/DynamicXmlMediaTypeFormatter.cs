using MockEngine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;

namespace MockEngine.Http.Formatters
{
    public class DynamicXmlMediaTypeFormatter : XmlMediaTypeFormatter
    {
        public static readonly Type DynamicYamlType = typeof(Dictionary<object, object>);
        private JsonSerializer _jsonSerializer = new JsonSerializer();
        public DynamicXmlMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
        }
        public override bool CanReadType(Type type)
        {
            if (type == typeof(DynamicObject))
            {
                return true;
            }
            else if (type == typeof(object))
            {
                return true;
            }
            return false;
        }
        public override bool CanWriteType(Type type)
        {
            if (type == DynamicYamlType)
            {
                return true;
            }
            else if (type == typeof(object))
            {
                return true;
            }
            else if ( type == typeof(DynamicObject))
            {
                return true;
            }
            return false;
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            if (type == typeof(DynamicObject) || type == typeof(object))
            {
                string textContent = await content.ReadAsStringAsync();
                return DynamicXml.Parse(textContent);
            }
            return base.ReadFromStreamAsync(type, readStream, content, formatterLogger, cancellationToken);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            if (type == DynamicYamlType || value is Dictionary<object,object>)
            {
                var writer = new StringWriter();
                _jsonSerializer.Serialize(writer, value);
                var reader = new StringReader(writer.ToString());
                var dynamicValue = _jsonSerializer.Deserialize(reader, typeof(object));
                XmlDocument doc = JsonConvert.DeserializeXmlNode(dynamicValue.ToString(),"data");
                var useDocElement = true;
                if (useDocElement)
                {
                    using (var xmlWriter = new XmlTextWriter(writeStream, Encoding.UTF8))
                    {
                        doc.WriteTo(xmlWriter);
                        writer.Flush();
                    }
                }
                else
                {
                    using (var streamWriter = new StreamWriter(writeStream, Encoding.UTF8))
                    {
                        streamWriter.Write(doc.DocumentElement.InnerXml);
                        streamWriter.Flush();
                    }
                }
                return;
            }
            await base.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
        }

        protected override object GetSerializer(Type type, object value, HttpContent content)
        {
            return base.GetSerializer(type, value, content);
        }

        public override DataContractSerializer CreateDataContractSerializer(Type type)
        {
            return base.CreateDataContractSerializer(type);
        }
    }
}
