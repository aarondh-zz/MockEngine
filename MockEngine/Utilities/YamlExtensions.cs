using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MockEngine.Utilities
{
    public static class YamlExtensions
    {
        private static Deserializer _deserializer;
        private static Serializer _serializer;
        static YamlExtensions()
        {
            var deserializerBuilder = new DeserializerBuilder();

            _deserializer = deserializerBuilder
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var serializerBuilder = new SerializerBuilder();

            _serializer = serializerBuilder
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithTypeInspector<DynamicXmlTypeInspector>((typeInspector) => { return new DynamicXmlTypeInspector(typeInspector);  })
                .Build();
        }
        public static string ToYamlString(this object graph)
        {
            try
            {
                return _serializer.Serialize(graph);
            }
            catch
            {
                return null;
            }
        }

        public static T DeserializeYaml<T>(this string yaml)
        {
            try
            {
                return _deserializer.Deserialize<T>(yaml);
            }
            catch
            {
                return default(T);
            }
        }
        public static object DeserializeYaml(this string yaml, Type type)
        {
            try
            {
                return _deserializer.Deserialize(yaml, type);
            }
            catch
            {
                return null;
            }
        }
    }
}
