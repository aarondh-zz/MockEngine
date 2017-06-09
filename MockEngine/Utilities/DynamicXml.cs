using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MockEngine.Utilities
{
    [Serializable]
    public class DynamicXml : DynamicObject, ISerializable
    {
        XElement _root;
        private DynamicXml(XElement root)
        {
            _root = root;
        }
        private DynamicXml()
        {
        }

        public static DynamicXml Parse(string xmlString)
        {
            return new DynamicXml(XDocument.Parse(xmlString).Root);
        }

        public static DynamicXml Load(string filename)
        {
            return new DynamicXml(XDocument.Load(filename).Root);
        }
        private bool TryGetMember(string memberName, out object result)
        {
            result = null;
            if (_root != null)
            {
                var att = _root.Attribute(memberName);
                if (att != null)
                {
                    result = att.Value;
                    return true;
                }

                var nodes = _root.Elements(memberName);
                if (nodes.Count() > 1)
                {
                    result = nodes.Select(n => n.HasElements ? (object)new DynamicXml(n) : n.Value).ToList();
                    return true;
                }

                var node = _root.Element(memberName);
                if (node != null)
                {
                    result = node.HasElements ? (object)new DynamicXml(node) : node.Value;
                    return true;
                }
            }
            return false;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (TryGetMember(binder.Name, out result))
            {
                return true;
            }
            return base.TryGetMember(binder, out result);
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return base.TrySetMember(binder, value);
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var memberNames = new List<string>();
            var baseNames = base.GetDynamicMemberNames();
            memberNames.AddRange(baseNames);
            var attribute = _root.FirstAttribute;
            while ( attribute != null)
            {
                memberNames.Add(attribute.Name.LocalName);
                attribute = attribute.NextAttribute;
            }
            foreach ( var element in _root.Elements())
            {
                memberNames.Add(element.Name.LocalName);
            }
            return memberNames;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            object value;
            info.FullTypeName = _root.Name.LocalName;
            foreach (var memberName in GetDynamicMemberNames())
            {
                if ( TryGetMember(memberName, out value))
                {
                    info.AddValue(memberName, value);
                }
            }
        }
        public override string ToString()
        {
            return _root == null ? "" : _root.ToString();
        }
    }
}
