using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Xml;

namespace MockEngine.Utilities
{
    [Serializable]
    public class DynamicXml : DynamicObject, ISerializable
    {
        public const string TextMemberName = "__text";
        public const string AttributePrefix = "_";
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
        public bool TryGetMember(string memberName, out object result)
        {
            result = null;
            if (_root != null)
            {
                if ( IsAttributeName(memberName))
                {
                    var att = _root.Attribute(ToAttributeName(memberName));
                    if (att != null)
                    {
                        result = att.Value;
                        return true;
                    }
                }
                if ( IsTextName(memberName))
                {
                    result = _root.Value;
                    return true;
                }
                var matchingElements = new List<XElement>();
                var node = _root.FirstNode;
                while (node != null)
                {
                    if ( node.NodeType == XmlNodeType.Element )
                    {
                        var element = node as XElement;
                        if ( element.Name.LocalName == memberName )
                        {
                            matchingElements.Add(node as XElement);
                        }
                    }
                    node = node.NextNode;
                }
                if (matchingElements.Count() > 1)
                {
                    result = matchingElements.Select(n => n.HasElements || n.HasAttributes ? (object)new DynamicXml(n) : n.Value).ToList();
                    return true;
                }
                else
                {
                    var matchedElement = matchingElements.First();
                    result = matchedElement.HasElements || matchedElement.HasAttributes ? (object)new DynamicXml(matchedElement) : matchedElement.Value;
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
        public static bool IsAttributeName(string memberName)
        {
            return memberName != null && !IsTextName(memberName) && memberName.StartsWith(AttributePrefix);
        }
        public static string ToAttributeName(string memberName)
        {
            if ( memberName == null)
            {
                return null;
            }
            return memberName.Substring(AttributePrefix.Length);
        }
        public static bool IsTextName(string memberName)
        {
            return memberName == TextMemberName;
        }
        public bool TrySetMember(string memberName, object value)
        {
            if (_root != null)
            {
               if ( IsAttributeName(memberName))
                {
                    var attributeName = ToAttributeName(memberName);
                    if ( value == null )
                    {
                        var attribute = _root.Attribute(attributeName);
                        if ( attribute != null)
                        {
                            attribute.Remove();
                        }
                    }
                    else
                    {
                        _root.SetAttributeValue(memberName, value);
                    }
                    return true;
                }
                if ( IsTextName(memberName))
                {
                    _root.SetValue(value);
                    return true;
                }
                var nodes = _root.Elements(memberName);
                foreach (var n in nodes)
                {
                    n.Remove();
                }
                _root.Add(new XElement(memberName,XElement.Parse(value.ToXmlString())));
                return true;
            }
            return false;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (TrySetMember(binder.Name, value))
            {
                return true;
            }
            return base.TrySetMember(binder, value);
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var memberNames = new List<string>();
            var baseNames = base.GetDynamicMemberNames();
            memberNames.AddRange(baseNames);
            var attribute = _root.FirstAttribute;
            while (attribute != null)
            {
                memberNames.Add(AttributePrefix + attribute.Name.LocalName);
                attribute = attribute.NextAttribute;
            }
            var node = _root.FirstNode;
            while (node != null)
            {
                switch(node.NodeType)
                {
                    case XmlNodeType.Element:
                        memberNames.Add((node as XElement).Name.LocalName);
                        break;
                    case XmlNodeType.Text:
                        memberNames.Add(TextMemberName);
                        break;
                }
                node = node.NextNode;
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
    class DynamicXmlPropertyDescriptor : IPropertyDescriptor
    {
        public bool CanWrite
        {
            get
            {
                return true;
            }
        }
        public string Name { get; set; }

        public int Order { get; set; }

        public ScalarStyle ScalarStyle { get; set; }

        public Type Type { get; set; }

        Type IPropertyDescriptor.TypeOverride { get; set; }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return null;
        }

        public IObjectDescriptor Read(object target)
        {
            var dynamicXml = target as DynamicXml;
            object value;
            if (dynamicXml.TryGetMember(Name, out value))
            {
                return new ObjectDescriptor(value, value.GetType(), Type, ScalarStyle);
            }
            return null;
        }

        public void Write(object target, object value)
        {
            var dynamicXml = target as DynamicXml;
        }

        T IPropertyDescriptor.GetCustomAttribute<T>()
        {
            return null;
        }
    }

    public class DynamicXmlTypeInspector : ITypeInspector
    {
        public static ITypeInspector _baseTypeInspector;
        public DynamicXmlTypeInspector(ITypeInspector baseTypeInspector)
        {
            _baseTypeInspector = baseTypeInspector;
        }
        public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            if ( type != typeof(DynamicXml))
            {
                return _baseTypeInspector.GetProperties(type, container);
            }
            var properties = new List<IPropertyDescriptor>();
            var dynamicXml = container as DynamicXml;
            var order = 0;
            foreach (var memberName in dynamicXml.GetDynamicMemberNames())
            {
                var property = GetProperty(dynamicXml, memberName, true, order++);
                if (property != null)
                {
                    properties.Add(property);
                }
            }
            return properties;
        }
        private IPropertyDescriptor GetProperty(DynamicXml container, string name, bool ignoreUnmatched, int order)
        {
            
            object value;
            if (container.TryGetMember(name, out value))
            {
                if (value != null)
                {
                    return new DynamicXmlPropertyDescriptor() { Name = name, Order = order, ScalarStyle = ScalarStyle.Any, Type = value.GetType() };
                }
            }
            return null;
        }
        public IPropertyDescriptor GetProperty(Type type, object container, string name, bool ignoreUnmatched)
        {
            if (type != typeof(DynamicXml))
            {
                return _baseTypeInspector.GetProperty(type, container, name, ignoreUnmatched);
            }
            return GetProperty(container as DynamicXml, name, ignoreUnmatched, 0);
        }
    }
}
