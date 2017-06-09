using MockEngine.Http.Configuration;
using MockEngine.Http.Formatters;
using MockEngine.Http.Interfaces;
using MockEngine.Http.Utilities;
using MockEngine.Interfaces;
using MockEngine.Resolvers;
using MockEngine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MockEngine.Http
{
    public class MockMessageHandler : DelegatingHandler
    {
        private IMockHandlerSettings _settings;
        private Regex _whenMatcher;
        private IMockEngine _mockEngine;
        public MockMessageHandler(IMockHandlerSettings settings = null)
        {
            if ( settings == null)
            {
                _settings = MockMessageHandlerSettings.Settings;
            }
            else
            {
                _settings = settings;
            }

            _whenMatcher = new Regex(_settings.When, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            var mockEngineFactory = new MockEngineFactory();
            mockEngineFactory.Initialize(_settings);
            _mockEngine = mockEngineFactory.CreateMockEngine(_settings.Name);
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_settings.Enabled)
            {
                try
                {
                    var pathAndQuery = request.RequestUri.PathAndQuery;
                    if (_whenMatcher == null || _whenMatcher.IsMatch(pathAndQuery))
                    {
                        var dynamicParameters = new ExpandoObject();
                        dynamicParameters.AddProperty("Request", request);
                        foreach (var property in request.Properties)
                        {
                            dynamicParameters.AddProperty(property.Key, property.Value);
                        }
                        var parameters = request.RequestUri.ParseQueryString();
                        foreach (string parameterName in parameters)
                        {
                            dynamicParameters.AddProperty(parameterName, parameters[parameterName]);
                        }
                        var scenario = _mockEngine.FindScenario(pathAndQuery);
                        if (scenario == null)
                        {
                            return await base.SendAsync(request, cancellationToken);
                        }
                        if (request.Content.Headers.ContentType != null)
                        {
                            var contentType = request.Content.Headers.ContentType.MediaType;
                            if (contentType == "application/xml")
                            {
                                if (scenario.RequestType == typeof(DynamicObject))
                                {
                                    var content = await request.Content.ReadAsStringAsync();
                                    dynamicParameters.AddProperty(scenario.RequestParameterName, DynamicXml.Parse(content));
                                }
                                else
                                {
                                    var contentGraph = await request.Content.ReadAsAsync(scenario.RequestType);
                                    dynamicParameters.AddProperty(scenario.RequestParameterName, contentGraph);
                                }
                            }
                            else if (contentType == "application/yaml")
                            {
                                // TBD: Note that I should be able to do this with the YamlMediaTypeFormatter
                                // but it is currently being ignored
                                var content = await request.Content.ReadAsStringAsync();
                                if (scenario.RequestType == typeof(DynamicObject))
                                {
                                    dynamicParameters.AddProperty(scenario.RequestParameterName, content.DeserializeYaml<object>());
                                }
                                else
                                {
                                    dynamicParameters.AddProperty(scenario.RequestParameterName, content.DeserializeYaml(scenario.RequestType));
                                }
                            }
                            else
                            {
                                var contentGraph = await request.Content.ReadAsAsync(scenario.RequestType);
                                dynamicParameters.AddProperty(scenario.RequestParameterName, contentGraph);
                            }
                        }
                        var result = _mockEngine.Invoke(scenario.Name, dynamicParameters);
                        if (result.Success)
                        {
                            var response = new HttpResponseMessage(result.StatusCode) { ReasonPhrase = result.ReasonPhrase };
                            response.RequestMessage = request;
                            if (result.Content != null)
                            {
                                Type contentType = result.Content.GetType();
                                if (WillAcceptType(request.Headers.Accept, "application/xml"))
                                {
                                    response.Content = new StringContent(SerializeAsXml(result.Content, Encoding.UTF8), Encoding.UTF8, "application/xml");
                                }
                                else if (WillAcceptType(request.Headers.Accept, "application/json"))
                                {
                                    response.Content = new ObjectContent(contentType, result.Content, new JsonMediaTypeFormatter(), "application/json");
                                }
                                else if (WillAcceptType(request.Headers.Accept, "application/yaml") || WillAcceptType(request.Headers.Accept, "*/*"))
                                {
                                    response.Content = new ObjectContent(contentType, result.Content, new YamlMediaTypeFormatter(), "application/yaml");
                                }
                                else
                                {
                                    response.Content = new StringContent("response media type is not supported");
                                    response.StatusCode = HttpStatusCode.UnsupportedMediaType;
                                    response.ReasonPhrase = "response media type is not supported";
                                }
                            }
                            return response;
                        }
                    }

                }
                catch( Exception e)
                {
                    Trace.WriteLine(e);
                    var reasonPhrase = e.Message.Replace("\n", " ").Replace("\r", " "); //cannot contain new lines
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = reasonPhrase };
                    response.RequestMessage = request;
                    response.Content = new StringContent(e.ToString(), Encoding.UTF8, "text/plain");
                    return response;
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
        private string SerializeAsXml(object graph, Encoding encoding)
        {
            var serializer = GetXmlSerializer(graph.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, graph);
                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);
                var reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
        }
        private JsonSerializer GetJsonSerializer()
        {
            return new JsonSerializer();
        }
        private string SerializeAsJson(object graph, Encoding encoding)
        {
            var serializer = GetJsonSerializer();
            var stringWriter = new StringWriter();
            var jsonWriter = new JsonTextWriter(stringWriter);
            serializer.Serialize(jsonWriter, graph);
            return stringWriter.ToString();
        }
        private ISerializer GetXmlSerializer(Type type)
        {
            if ( DataContractHelper.IsDataContractType(type))
            {
                return new Utilities.DataContractSerializer(type);
            }
            if (type == typeof(DynamicXml))
            {
                return new DynamicXmlSerializer();
            }
            if (typeof(IDictionary<object, object>).IsAssignableFrom(type))
            {
                return new XmlDictionarySerializer("data");
            }
            else
            {
                return new XmlSerializer(type);
            }
        }
        private bool WillAcceptType(HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> mediaTypes, string contentType)
        {
            foreach (var mediaType in mediaTypes)
            {
                if (string.Equals(mediaType.MediaType, contentType, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
