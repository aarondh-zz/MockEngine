using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Utilities
{
    public class DataContractHelper
    {

        public static bool IsDataContractType(Type type)
        {
            return type.GetCustomAttribute<DataContractAttribute>() != null;
        }
        public static bool IsDataContract(object graph)
        {
            if ( graph == null)
            {
                return false;
            }
            return IsDataContractType(graph.GetType());
        }
    }
}
