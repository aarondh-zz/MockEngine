using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using MockEngine.Models;
using YamlDotNet.Core;
using System.Net;
using System.Data;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using System.Collections;
using System.Dynamic;
using MockEngine.Utilities;
using MockEngine.Interfaces;

namespace MockEngine.Internal
{
    internal class MockEngine : IMockEngine
    {
        private ScenarioManager _scenarioManager;
        private ITypeResolver _typeResolver;
        private ILogProvider _logProvider;
        private IMockContext _context;
        public MockEngine(IMockContext context, string engineName, IScenarioResolver scenarioResolver)
        {
            _context = context;
            _typeResolver = context.TypeResolver;
            _logProvider = context.LogProvider;
            _scenarioManager = new ScenarioManager(context, scenarioResolver);
            this.Name = engineName;
        }
        public string Name { get; }

        private string ToValueString(object value)
        {
            if ( value == null)
            {
                return "null";
            }
            else if ( value is string)
            {
                return "\"" + value.ToString().Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
            }
            else
            {
                return value.ToString();
            }
        }
        private void SetInputParameters(V8ScriptEngine scriptEngine, object dynamicProperties)
        {
            if (dynamicProperties != null)
            {
                if (dynamicProperties is IDictionary<string, object>)
                {
                    var propertyDictionary = dynamicProperties as IDictionary<string, object>;
                    foreach (var property in propertyDictionary)
                    {
                        if (property.Value.GetType().IsPrimitive || property.Value is string)
                        {
                            scriptEngine.Script[property.Key] = property.Value;
                        }
                        else
                        {
                            scriptEngine.AddHostObject(property.Key, property.Value);
                        }
                    }
                }
                else
                {
                    foreach (var property in dynamicProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        object value = property.GetValue(dynamicProperties);
                        if (value != null)
                        {
                            if (value.GetType().IsPrimitive || value is string)
                            {
                                scriptEngine.Script[property.Name] = value;
                            }
                            else
                            {
                                scriptEngine.AddHostObject(property.Name, value);
                            }
                        }
                        _logProvider.Verbose($"{property.Name} = {ToValueString(value)}");
                    }
                }
            }
        }
        private void SetGlobals(V8ScriptEngine scriptEngine, MockScenario scenario)
        {
            if (scenario.Global != null)
            {
                if (scenario.Global.Tables != null)
                {
                    scriptEngine.AddHostObject("tables", new Tables(_context, scenario.Global.Tables));
                }
            }
        }
        private class Scenario : IScenario
        {
            private Type _requestType;
            private IMockContext _context;
            private MockScenario _scenario;
            public Scenario( IMockContext context, MockScenario scenario)
            {
                _context = context;
                _scenario = scenario;
            }

            public string Description
            {
                get
                {
                    return _scenario.Description;
                }
            }

            public string Name
            {
                get
                {
                    return _scenario.Name;
                }
            }

            public string RequestParameterName
            {
                get
                {
                    return _scenario.Request?.ParameterName??"body";
                }
            }

            public Type RequestType
            {
                get
                {
                    if (_requestType == null)
                    {
                        var requestTypeName = _scenario?.Request?.Type;
                        if ( string.IsNullOrWhiteSpace(requestTypeName))
                        {
                            return typeof(DynamicObject);
                        }
                        _requestType = _context.TypeResolver.Resolve(requestTypeName);
                    }
                    return _requestType;
                }
            }
        }
        public IScenario FindScenario(string path)
        {
            var mockScenario = _scenarioManager.FindScenario(path);
            return mockScenario == null ? null : new Scenario(_context, mockScenario);
        }
        public IMockEngineResponse Invoke(string scenarioName, object dynamicProperties = null)
        {
            if (scenarioName == null)
            {
                throw new ArgumentNullException(nameof(scenarioName));
            }

            _logProvider.Verbose($"Invoking scenario \"" + scenarioName + "\"");

            var scenario = _scenarioManager.GetScenario(scenarioName);

            if ( scenario == null)
            {
                throw new ArgumentOutOfRangeException(nameof(scenarioName),$"scenario \"{scenarioName}\" was not found.");
            }

            using (var scriptEngine = new V8ScriptEngine())
            {
                scriptEngine.AllowReflection = true;

                scriptEngine.Script["scenarioName"] = scenarioName;

                SetInputParameters(scriptEngine, dynamicProperties);

                SetGlobals(scriptEngine, scenario);

                if ( scenario.Global != null && !string.IsNullOrWhiteSpace(scenario.Global.Before))
                {
                    try
                    {
                        scriptEngine.Execute(scenario.Global.Before);
                    }
                    catch (Exception e)
                    {
                        throw new MockEngineException(this.Name, scenarioName, $"error executing global Before code: {e.Message}", e);
                    }
                }

                MockEngineResponse response = null;

                foreach ( var action in scenario.Actions)
                {
                    try
                    {
                        if (action.When == null)
                        {
                            response = ExecuteAction(scriptEngine, scenarioName, action);
                            break;
                        }
                        else
                        {
                            var caseResult = scriptEngine.Evaluate(action.When);
                            if (caseResult is bool)
                            {
                                if ((bool)caseResult)
                                {
                                    response = ExecuteAction(scriptEngine, scenarioName, action);
                                    break;
                                }
                            }
                            else
                            {
                                _logProvider.Warning($"action: {action} case expression did not return a boolean result");
                            }
                        }

                    }
                    catch (MockEngineException e)
                    {
                        var message = $"action: {action} exception: {e.Message}";
                        _logProvider.Error(message);
                        throw new MockEngineException(this.Name, scenarioName, message, e);
                    }
                    catch (Exception e)
                    {
                        var message = $"action: {action} exception: {e.Message}";
                        _logProvider.Error(message);
                        throw new MockEngineException(this.Name, scenarioName, message, e);
                    }
                }
                if (scenario.Global != null && !string.IsNullOrWhiteSpace(scenario.Global.After))
                {
                    try
                    {
                        scriptEngine.Execute(scenario.Global.After);
                    }
                    catch (Exception e)
                    {
                        throw new MockEngineException(this.Name, scenarioName, $"error executing global After code: {e.Message}", e);
                    }
                }

                if (response != null)
                {
                    return response;
                }
            }

            var reasonPhrase = $"scenario: \"{scenarioName}\" did not contain any matching actions for this request.";

            _logProvider.Error(reasonPhrase);

            return new MockEngineResponse() { ReasonPhrase = reasonPhrase };
        }
        private MockEngineResponse ExecuteAction(V8ScriptEngine scriptEngine, string scenarioName, MockAction action)
        {
            _logProvider.Information($"executing action: {action}");
            var response = new MockEngineResponse() { Success = true };
            if (action.Before != null)
            {
                try
                {
                    scriptEngine.Execute(action.Before);
                }
                catch (Exception e)
                {
                    throw new MockEngineException(this.Name, scenarioName, $"error executing Before code: {e.Message}", e);
                }
            }
            if (action.Response != null)
            {
                response = BuildResponse(scriptEngine, scenarioName, action);
                if (response != null)
                {
                    scriptEngine.AddHostObject("response", response);
                }
            }
            if (action.After != null)
            {
                try
                {
                    scriptEngine.Execute(action.After);
                }
                catch (Exception e)
                {
                    throw new MockEngineException(this.Name, scenarioName, $"error executing After code: {e.Message}", e);
                }
            }
            if (action.Log != null)
            {
                _logProvider.Information(ApplyExpressions(scriptEngine,action.Log, false));
            }
            return response;
        }

        private MockEngineResponse BuildMockEngineResponse(V8ScriptEngine scriptEngine, string scenarioName, MockAction action)
        {
            var mockResponse = action.Response;
            var response = new MockEngineResponse() { Success = true };
            if (!string.IsNullOrWhiteSpace(mockResponse.Reason))
            {
                response.ReasonPhrase = mockResponse.Reason;
            }
            if (string.IsNullOrWhiteSpace(mockResponse.StatusCode))
            {
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                HttpStatusCode statusCode;
                int intStatusCode;
                if (Enum.TryParse<HttpStatusCode>(mockResponse.StatusCode, out statusCode))
                {
                    response.StatusCode = statusCode;
                }
                else if (int.TryParse(mockResponse.StatusCode, out intStatusCode))
                {
                    var value = Enum.ToObject(typeof(HttpStatusCode), intStatusCode);
                    if (Enum.IsDefined(typeof(HttpStatusCode), value))
                    {
                        response.StatusCode = (HttpStatusCode)value;
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        response.ReasonPhrase = $"action {action} specified an invalid status code \"{mockResponse.StatusCode}\"";
                        _logProvider.Warning(response.ReasonPhrase);
                    }
                }
                else
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    _logProvider.Warning($"action {action} specified an invalid status code \"{mockResponse.StatusCode}\"");
                }
            }
            return response;
        }

        private MockEngineResponse BuildResponse(V8ScriptEngine scriptEngine, string scenarioName, MockAction action)
        {
            try
            {
                var response = BuildMockEngineResponse(scriptEngine, scenarioName, action);
                var mockResponse = action.Response;
                if (mockResponse.Body != null || mockResponse.BodyXml != null)
                {
                    Type responseType;
                    if (mockResponse.BodyType == null)
                    {
                        responseType = typeof(object);
                    }
                    else
                    {
                        responseType = _typeResolver.Resolve(action.Response.BodyType);
                        if (responseType == null)
                        {
                            var message = $"response type \"{mockResponse.BodyType}\" not found.";
                            _logProvider.Error(message);
                            throw new MockEngineException(this.Name, scenarioName, message);
                        }
                    }
                    if (mockResponse.Body == null)
                    {
                        if (mockResponse.BodyXml == null)
                        {
                            _logProvider.Warning("response body or xml was not defined");
                        }
                        else
                        {
                            var processedXml = ApplyExpressions(scriptEngine, mockResponse.BodyXml, false);
                            response.Content = DeserializeXml(processedXml, responseType);
                        }
                    }
                    else
                    {
                        var processedBody = ApplyYamlExpressions(scriptEngine, mockResponse.Body);
                        if ( responseType == typeof(object))
                        {
                            response.Content = processedBody;
                        }
                        else
                        {
                            var processedYaml = _scenarioManager.Serialize(processedBody);
                            response.Content = _scenarioManager.Deserialize(processedYaml, responseType);
                        }
                    }
                }
                return response;
            }
            catch (YamlDotNet.Core.SyntaxErrorException e)
            {
                var message = "syntax error building response: {e.Message}";
                _logProvider.Error(message);
                throw new MockEngineException(this.Name, scenarioName, message, e);
            }
            catch (YamlException e)
            {
                var message = $"error deserializing response: {e.InnerException.Message} at {e.Source}";
                _logProvider.Error(message);
                throw new MockEngineException(this.Name, scenarioName, message, e);
            }
            catch (Exception e)
            {
                var message = $"error building response: {e.Message}";
                _logProvider.Error(message);
                throw new MockEngineException(this.Name, scenarioName, message, e);
            }
        }
        private object DeserializeXml( string xmlText, Type type)
        {
            var serializer = new DataContractSerializer(type);
            using (var reader = new StringReader(xmlText))
            {
                var xmlReader = XmlReader.Create(reader);
                return serializer.ReadObject(xmlReader);
            }
        }
        private static Regex _expressionMatcher = new Regex(@"\<\<(.*?)\>\>", RegexOptions.Compiled | RegexOptions.Multiline);
        /// <summary>
        /// process and apply any expression found inside double << >> 
        /// i.e. "<" "<" (javascript expression) ">" ">"
        /// </summary>
        /// <param name="scriptEngine"></param>
        /// <param name="textWithExpressions"></param>
        /// <param name="outputYaml">When true the yaml representation of the expression is injected</param>
        /// <returns></returns>
        private string ApplyExpressions(V8ScriptEngine scriptEngine, string textWithExpressions, bool outputYaml)
        {
            return _expressionMatcher.Replace(textWithExpressions, (match) =>
            {
                var expression = match.Groups[1].Value;
                if ( string.IsNullOrWhiteSpace(expression))
                {
                    return outputYaml ? "~" : "";
                }
                else
                {
                    try
                    {
                        var result = scriptEngine.Evaluate(expression);
                        if (result == null)
                        {
                            return outputYaml ? "~" : "";
                        }
                        else
                        {
                            var text = result.ToString();
                            if (outputYaml)
                            {
                                if ( result is string)
                                {
                                    return "\"" + text.Replace("\"", "\"\"") + "\"";
                                }
                            }
                            return text;
                        }
                    }
                    catch (Exception e)
                    {
                        _logProvider.Warning($"Error processing expression <<{match.Value}>>: {e.Message}");
                        return outputYaml ? "~" : "";
                    }
                }
            });
        }
        private object ProcessEach(V8ScriptEngine scriptEngine, Dictionary<object, object> parameters)
        {
            var list = new List<object>();
            object variableNameObj;
            if (parameters.TryGetValue("var", out variableNameObj))
            {
                if (variableNameObj is string) {
                    var variableName = variableNameObj as string;
                    object inListTextObj;
                    if (parameters.TryGetValue("in", out inListTextObj))
                    {
                        if (inListTextObj is string)
                        {
                            object doObj;
                            if (parameters.TryGetValue("do", out doObj))
                            {
                                object inListObj = scriptEngine.Evaluate(inListTextObj as string);
                                if (inListObj is IEnumerable)
                                {
                                    foreach (var itemObj in (IEnumerable)inListObj)
                                    {
                                        scriptEngine.AddHostObject(variableName, itemObj);
                                        var itemResult = ApplyYamlExpressions(scriptEngine, doObj);
                                        list.Add(itemResult);
                                    }
                                }
                                else
                                {
                                    scriptEngine.AddHostObject(variableName, inListObj);
                                    var itemResult = ApplyYamlExpressions(scriptEngine, doObj);
                                    list.Add(itemResult);
                                }
                            }
                            return list;
                        }
                    }
                }
            }
            throw new Exception("Invalid $each");
        }
        private object ProcessIf(V8ScriptEngine scriptEngine, Dictionary<object, object> parameters)
        {
            object value = null;
            var testResult = scriptEngine.Evaluate(parameters["test"] as string);
            if ( testResult is bool && (bool)testResult)
            {
                if ( parameters.TryGetValue("then", out value) ) {
                    return ApplyYamlExpressions( scriptEngine, value);
                }
            }
            else if (parameters.TryGetValue("else", out value))
            {
                return ApplyYamlExpressions(scriptEngine, value);
            }

            return value;
        }
        private object ApplyYamlExpressions(V8ScriptEngine scriptEngine, object graph)
        {
            if (graph is Dictionary<object, object>)
            {
                var newDictionary = new Dictionary<object, object>();
                var dictionary = (Dictionary<object, object>)graph;
                foreach (var keyValuePair in dictionary)
                {
                    var name = keyValuePair.Key as string;
                    if ( name.StartsWith("$"))
                    {
                        if ( dictionary.Count == 1)
                        {
                            return ApplyYamlExpressions(scriptEngine, keyValuePair);
                        }
                    }
                    else
                    {
                        newDictionary[keyValuePair.Key] = ApplyYamlExpressions(scriptEngine, keyValuePair.Value);
                    }
                }
                return newDictionary;
            }
            else if (graph is KeyValuePair<object, object>)
            {
                var pair = (KeyValuePair<object, object>)graph;
                var name = pair.Key.ToString();
                if (name == "$each")
                {
                    if (pair.Value is Dictionary<object, object>)
                    {
                        return ProcessEach(scriptEngine, (Dictionary<object, object>)pair.Value);
                    }
                    throw new Exception("Invalid $each");
                }
                else if (name == "$if")
                {
                    if (pair.Value is Dictionary<object, object>)
                    {
                        return ProcessIf(scriptEngine, (Dictionary<object, object>)pair.Value);
                    }
                    throw new Exception("Invalid $if");
                }
                else if (graph is List<object>)
                {
                    var newList = new List<object>();
                    var list = (List<object>)graph;
                    foreach (var item in list)
                    {
                        newList.Add(ApplyYamlExpressions(scriptEngine, item));
                    }
                    return newList;
                }
            }
            else if (graph is string)
            {
                return ApplyExpressions(scriptEngine, graph.ToString(), false);
            }

            return graph;
        }
    }
}
