using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MockEngine.Utilities;
using System.Dynamic;
using System.Net;
using System.Threading;

namespace MockEngine.Http.Formatters
{
    public class YamlMediaTypeFormatter : MediaTypeFormatter
    {
        public YamlMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/yaml"));
        }

        public YamlMediaTypeFormatter(
               MediaTypeMapping mediaTypeMapping) : this() {

            MediaTypeMappings.Add(mediaTypeMapping);
        }
        public YamlMediaTypeFormatter(
             IEnumerable<MediaTypeMapping> mediaTypeMappings) : this() {

            foreach (var mediaTypeMapping in mediaTypeMappings)
            {
                MediaTypeMappings.Add(mediaTypeMapping);
            }
        }
        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew<object>(() =>
            {
                using (StreamReader reader = new StreamReader(readStream))
                {
                    if (type == typeof(DynamicObject))
                    {
                        return reader.ReadToEnd().DeserializeYaml<object>();
                    }
                    else
                    {
                        return reader.ReadToEnd().DeserializeYaml(type);
                    }
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => {
                using (StreamWriter writer = new StreamWriter(writeStream))
                {
                    if (value != null)
                    {
                        writer.Write(value.ToYamlString());
                    }
                }
            });
        }
    }
}
