using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Http.Utilities
{
    public static class DynamicNameValueCollectionExtensions
    {
        public static void AddHeaders( this DynamicNameValueCollection collection, HttpRequestHeaders headers)
        {
            foreach( var header in headers)
            {
                foreach( var value in header.Value)
                {
                    collection.Add(header.Key, value);
                }
            }
        }

        public static void AddSplitValues(this DynamicNameValueCollection collection, string values, string prefix = "")
        {
            var components = values.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var component in components)
            {
                var subcomponents = component.Split(new char[] { '=' });
                var name = prefix + subcomponents[0];
                if (subcomponents.Length == 1)
                {
                    collection.Add(name, "true");
                }
                else if (subcomponents.Length == 2)
                {
                    collection.Add(name, subcomponents[1]);
                }
                else
                {
                    collection.Add(name, subcomponents.Skip(1).Aggregate((a,b)=> { return a + "=" + b; }));
                }
            }
        }
    }
}
