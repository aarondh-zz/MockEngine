using MockEngine.Http.Configuration;
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
                                    var serializer = new DataContractSerializer(scenario.RequestType);
                                    dynamicParameters.AddProperty(scenario.RequestParameterName, serializer.ReadObject(await request.Content.ReadAsStreamAsync()));
                                }
                            }
                            else if (contentType == "application/json")
                            {
                                var serializer = new JsonSerializer();
                                using (var reader = new StreamReader(await request.Content.ReadAsStreamAsync()))
                                {
                                    dynamicParameters.AddProperty(scenario.RequestParameterName, serializer.Deserialize(reader, scenario.RequestType));
                                }
                            }
                            else if (contentType == "application/yaml")
                            {
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
                                dynamicParameters.AddProperty(scenario.RequestParameterName, await request.Content.ReadAsStringAsync());
                            }
                        }
                        var result = _mockEngine.Invoke(scenario.Name, dynamicParameters);
                        if (result.Success)
                        {
                            var response = new HttpResponseMessage(result.StatusCode) { ReasonPhrase = result.ReasonPhrase };
                            response.RequestMessage = request;
                            if (result.Content != null)
                            {
                                Type responseType = result.Content.GetType();
                                if (WillAcceptType(request.Headers.Accept, "application/xml"))
                                {
                                    bool isDataContract = responseType.GetCustomAttribute<DataContractAttribute>() != null;
                                    if (isDataContract)
                                    {
                                        response.Content = new StringContent(SerializeDataContractAsXml(result.Content, Encoding.UTF8), Encoding.UTF8, "application/xml");
                                    }
                                    else
                                    {
                                        response.Content = new StringContent(SerializeAsXml(result.Content, Encoding.UTF8), Encoding.UTF8, "application/xml");
                                    }
                                }
                                else if (WillAcceptType(request.Headers.Accept, "application/json"))
                                {
                                    response.Content = new StringContent(SerializeAsJson(result.Content, Encoding.UTF8), Encoding.UTF8, "application/json");
                                }
                                else if (WillAcceptType(request.Headers.Accept, "application/yaml"))
                                {
                                    response.Content = new StringContent(result.Content.ToYamlString(), Encoding.UTF8, "application/yaml");
                                }
                                else if (WillAcceptType(request.Headers.Accept, "*/*"))
                                {
                                    response.Content = new StringContent(SerializeAsJson(result.Content, Encoding.UTF8), Encoding.UTF8, "application/json");
                                }
                                else
                                {
                                    throw new NotImplementedException();
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
        private DataContractSerializer GetDataContractSerializer(Type type)
        {
            return new DataContractSerializer(type);
        }
        private string SerializeDataContractAsXml(object graph, Encoding encoding)
        {
            var serializer = GetDataContractSerializer(graph.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, graph);
                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);
                var reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
        }
        private ISerializer GetXmlSerializer(Type type)
        {
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
