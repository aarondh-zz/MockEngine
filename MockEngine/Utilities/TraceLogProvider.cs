using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MockEngine.Utilities
{
    public class TraceLogProvider : ILogProvider
    {
        private class MessageTemplateParser
        {
            public class ParsedTemplate
            {
                private static Regex _matcher = new Regex(@"\{\{|\}\}|\{([^\{]+)\}", RegexOptions.Compiled);
                public string Template { get; private set; }
                public string Format { get; private set; }
                public List<string> Properties { get; private set; }
                public ParsedTemplate(string template)
                {
                    this.Template = template;
                    this.Properties = new List<string>();
                    this.Format = _matcher.Replace(template,(match)=>
                    {
                        if (match.Value == "{{")
                        {
                            return "{";
                        }
                        else if (match.Value == "}}")
                        {
                            return "}";
                        }
                        else
                        {
                            var text =  "{" + this.Properties.Count + "}";
                            this.Properties.Add(match.Groups[1].Value);
                            return text;
                        }
                    });
                }
            }
            private Dictionary<string, ParsedTemplate> _cachedParsedTemplates;
            public MessageTemplateParser()
            {
                _cachedParsedTemplates = new Dictionary<string, ParsedTemplate>();

            }
            public ParsedTemplate Parse( string messageTemplate )
            {
                ParsedTemplate parsedTemplate;
                if ( !_cachedParsedTemplates.TryGetValue(messageTemplate, out parsedTemplate))
                {
                    parsedTemplate = new ParsedTemplate(messageTemplate);
                }
                return parsedTemplate;
            }
        }
        private class MessageFormatter
        {
            public static MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();
            private Exception _exception;
            private object[] _propertyValues;
            private IDictionary<string, object> _properties;
            public string MessageTemplate { get; private set; }
            public string FormattedMessage
            {
                get
                {
                    var parsedTemplate = _messageTemplateParser.Parse(this.MessageTemplate);
                    return string.Format(parsedTemplate.Format, _propertyValues);
                }
            }
            public IDictionary<string,object> Properties {
                get
                {
                    if (_properties == null)
                    {
                        _properties = new Dictionary<string, object>();

                        if (_propertyValues != null)
                        {
                            var parsedTemplate = _messageTemplateParser.Parse(this.MessageTemplate);
                            int i = 0;
                            foreach (var propertyName in parsedTemplate.Properties)
                            {
                                if ( i < _propertyValues.Length )
                                {
                                    var propertyValue = _propertyValues[i];
                                    if ( propertyValue != null)
                                    {
                                        _properties[propertyName] = propertyValue;
                                    }
                                }
                                i++;
                            }
                        }
                    }
                    return _properties;
                }
            }
            public MessageFormatter(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                _exception = exception;
                this.MessageTemplate = messageTemplate;
                _propertyValues = propertyValues;

            }
            public override string ToString()
            {
                return this.FormattedMessage;
            }
        }
        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(null, messageTemplate, propertyValues), "Verbose");
        }
        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(exception, messageTemplate, propertyValues),"Verbose");
        }
        public void Information(string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(null, messageTemplate, propertyValues), "Information");
        }
        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(exception, messageTemplate, propertyValues), "Information");
        }
        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(null, messageTemplate, propertyValues), "Warning");
        }
        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(exception, messageTemplate, propertyValues), "Warning");
        }
        public void Error(string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(null, messageTemplate, propertyValues), "Error");
        }
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Trace.WriteLine(new MessageFormatter(exception, messageTemplate, propertyValues), "Error");
        }

        public void Initialize(IMockContext context)
        {
        }
    }
}
