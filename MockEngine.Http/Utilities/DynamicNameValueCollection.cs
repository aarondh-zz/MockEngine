using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Http.Utilities
{
    public class DynamicNameValueCollection : DynamicObject
    {
        NameValueCollection _headers;
        public DynamicNameValueCollection()
        {
            _headers = new NameValueCollection();
        }
        public DynamicNameValueCollection(NameValueCollection collection) : this()
        {
            foreach (string name in collection)
            {
                Add(name, collection[name]);
            }
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _headers.AllKeys;
        }
        public void Add( string parameterName, string parameterValue)
        {
            _headers.Add(ToValidPropertyName(parameterName), parameterValue);
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _headers[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Add(binder.Name, value == null ? "" : value.ToString());
            return true;
        }

        public static string ToValidPropertyName( string text)
        {
            StringBuilder propertyName = new StringBuilder();
            if ( text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            bool isFirst = true;
            foreach( char c in text)
            {
                if ( char.IsLetter(c) || c == '_')
                {
                    propertyName.Append(c);
                    isFirst = false;
                }
                else if ( char.IsDigit(c))
                {
                    if ( isFirst )
                    {
                        propertyName.Append('_');
                    }
                    isFirst = false;
                    propertyName.Append(c);
                }
                else
                {
                    //ignore it
                }

            }
            return propertyName.ToString();
        }
    }
}
