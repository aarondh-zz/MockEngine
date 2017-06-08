using MockEngine.Http.Configuration;
using MockEngine.Resolvers;
using MockEngine.Http.Utilities;
using System;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using MockEngine.Utilities;
using MockEngine.Http.Interfaces;
using MockEngine.Interfaces;

namespace MockEngine.Http
{
    public class MockHttpHandler : IHttpHandler
    {
        private IMockHandlerSettings _settings;
        private Regex _whenMatcher;
        private IMockEngine _mockEngine;
        public MockHttpHandler()
        {
            var mockEngineFactory = new MockEngineFactory();
            _settings = MockHttpHandlerSettings.Settings;
            if ( !string.IsNullOrEmpty(_settings.When) )
            {
                _whenMatcher = new Regex(_settings.When, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            }
            mockEngineFactory.Initialize(_settings);
            _mockEngine = mockEngineFactory.CreateMockEngine(_settings.Name);
        }
        public void ProcessRequest( HttpContext context)
        {
            if ( _settings.Enabled )
            {
                var request = context.Request;
                if (_whenMatcher == null ||  _whenMatcher.IsMatch(request.RawUrl))
                {
                    dynamic dynamicParameters = new ExpandoObject();
                    dynamicParameters.Request = request;
                    var queryString = request.QueryString;
                    foreach ( string parameterName in queryString)
                    {
                        dynamicParameters[parameterName] = queryString[parameterName];
                    }
                    var scenario = _mockEngine.FindScenario(request.RawUrl);
                    if ( scenario == null)
                    {
                        return; //do nothing
                    }
                    if (context.Request.ContentType == "text/xml")
                    {
                        if (scenario.RequestType == typeof(DynamicObject))
                        {
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                            {
                                dynamicParameters[scenario.RequestParameterName] = DynamicXml.Parse(reader.ReadToEnd());
                            }
                        }
                        else
                        {
                            var serializer = new DataContractSerializer(scenario.RequestType);
                            dynamicParameters[scenario.RequestParameterName] = serializer.ReadObject(request.InputStream);
                        }
                    }
                    else if (context.Request.ContentType == "application/json")
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            var jsonSerializer = new JsonSerializer();
                            dynamicParameters[scenario.RequestParameterName] = jsonSerializer.Deserialize(reader, scenario.RequestType);
                        }
                    }
                    else if (context.Request.ContentType == "application/yaml")
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            if (scenario.RequestType == typeof(DynamicObject))
                            {
                                dynamicParameters.AddProperty(scenario.RequestParameterName, reader.ReadToEnd().DeserializeYaml<object>());
                            }
                            else
                            {
                                dynamicParameters.AddProperty(scenario.RequestParameterName, reader.ReadToEnd().DeserializeYaml(scenario.RequestType));
                            }
                        }
                    }
                    else
                    {
                        var form = request.Form;
                        foreach (string parameterName in form)
                        {
                            dynamicParameters[parameterName] = form[parameterName];
                        }
                    }
                    var response = _mockEngine.Invoke(scenario.Name, dynamicParameters);
                    if (response.Success)
                    {
                        context.Response.StatusCode = response.StatusCode;
                        context.Response.Status = response.ReasonPhrase;
                        if (response.Content != null)
                        {
                            if (AcceptsType(context.Request.AcceptTypes, "application/xml"))
                            {
                                context.Response.ContentType = "application/xml";
                                GetXmlSerializer(response.Content.GetType()).WriteObject(context.Response.OutputStream, response.Content);
                            }
                            else if (AcceptsType(context.Request.AcceptTypes, "application/json"))
                            {
                                context.Response.ContentType = "application/json";
                                GetJsonSerializer(response.Content.GetType()).Serialize(context.Response.OutputStream, response.Content);
                            }
                            else if (AcceptsType(context.Request.AcceptTypes, "application/yaml"))
                            {
                                string content = response.Content.ToYamlString();
                                context.Response.ContentType = "application/yaml";
                                context.Response.Write(content);
                                GetJsonSerializer(response.Content.GetType()).Serialize(context.Response.OutputStream, response.Content);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        context.Response.End();
                    }
                }
            }
        }
        private DataContractSerializer GetXmlSerializer(Type type)
        {
            return new DataContractSerializer(type);
        }
        private JsonSerializer GetJsonSerializer(Type type)
        {
            return new JsonSerializer();
        }
        private bool AcceptsType( IEnumerable<string> acceptsTypes, string contentType)
        {
            foreach( var acceptsType in acceptsTypes)
            {
                if ( string.Equals(acceptsType, contentType, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
