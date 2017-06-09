using MockEngine.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MockEngine.Http.Utilities
{
    public class DataContractSerializer : ISerializer
    {
        System.Runtime.Serialization.DataContractSerializer _serializer;
        public DataContractSerializer(Type type)
        {
            _serializer = new System.Runtime.Serialization.DataContractSerializer(type);
        }

        public void Serialize(Stream stream, object graph)
        {
            _serializer.WriteObject(stream, graph);
        }
    }
}
