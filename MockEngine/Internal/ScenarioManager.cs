using System;
using MockEngine.Models;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using MockEngine.Interfaces;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MockEngine.Internal
{
    internal class ScenarioManager
    {
        /// <summary>
        /// Maps regex path patterns to scenarios
        /// </summary>
        private class Route
        {
            private Regex _pathMatcher;
            public Route( MockScenario scenario )
            {
                this.Scenario = scenario;
            }
            public MockScenario Scenario { get; }
            public bool IsMatch(string path)
            {
                if (string.IsNullOrWhiteSpace(Scenario.Path))
                {
                    return true;
                }
                else
                {
                    if ( _pathMatcher == null)
                    {
                        _pathMatcher = new Regex(Scenario.Path, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
                    }
                    return _pathMatcher.IsMatch(path);
                }
            }
            public override string ToString()
            {
                return $"{Scenario.Priority}:{Scenario.Name} /{(Scenario.Path == null ? "*." : Scenario.Path.Replace("/", "//"))}/ {Scenario.Request.Type} {Scenario.Request.ParameterName}";
            }
            public override bool Equals(object obj)
            {
                if ( obj is Route)
                {
                    return Scenario.Equals(((Route)obj).Scenario);
                }
                return false;
            }
            public override int GetHashCode()
            {
                return Scenario.GetHashCode();
            }
        }
        private class RouteKey
        {
            public float Priority { get; }
            public string Name { get; }

            public RouteKey(MockScenario mockScenario)
            {
                Priority = mockScenario.Priority;
                Name = mockScenario.Name;
            }
        }
        private class RouteKeyComparer : IComparer<RouteKey>
        {
            public static readonly IComparer<RouteKey> Instance = new RouteKeyComparer();
            private RouteKeyComparer() { }
            public int Compare(RouteKey a, RouteKey b)
            {
                var dif = a.Priority - b.Priority;
                if (dif == 0.0f)
                {
                    return string.Compare(a.Name, b.Name, true);
                }
                else if (dif < 0.0f)
                {
                    return -1;
                }
                return 1;
            }
        }
        private ConcurrentDictionary<string, MockScenario> _scenerioCache;
        private Deserializer _deserializer;
        private Serializer _serializer;
        private ILogProvider _logProvider;
        private IScenarioResolver _scenarioResolver;
        private object _lock = new object();
        private SortedList<RouteKey,Route> _routes;
        public ScenarioManager(IMockContext context, IScenarioResolver scenarioResolver)
        {
            _logProvider = context.LogProvider;

            _scenarioResolver = scenarioResolver;

            _scenarioResolver.RegisterChangeHandler(OnChangeHandler);

            _scenerioCache = new ConcurrentDictionary<string, MockScenario>();

            var deserializerBuilder = new DeserializerBuilder();

            _deserializer = deserializerBuilder
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var serialierBuilder = new SerializerBuilder();

            _serializer = serialierBuilder
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            _routes = new SortedList<RouteKey, Route>(RouteKeyComparer.Instance);

            PreloadAllScenarios();
        }
        /// <summary>
        /// load all scenarios
        /// </summary>
        public void PreloadAllScenarios()
        {
            _scenerioCache = new ConcurrentDictionary<string, MockScenario>();
            foreach( var scenarioName in _scenarioResolver.GetScenarioNames())
            {
                GetScenario(scenarioName);
            }
        }
        /// <summary>
        /// Delete a scenario by name
        /// </summary>
        /// <param name="scenarioName"></param>
        private void DeleteScenario( string scenarioName )
        {
            MockScenario mockScenario;
            if (_scenerioCache.TryRemove(scenarioName, out mockScenario))
            {
                _routes.Remove(new RouteKey(mockScenario));
            }
        }
        /// <summary>
        /// The Scenario Resolver reported that a scenario changes e.g. the scenario file on disk changed
        /// </summary>
        /// <param name="change"></param>
        private void OnChangeHandler(IScenarioChange change)
        {
            try
            {
                lock (_lock)
                {
                    switch (change.ChangeType)
                    {
                        case ScenarioChangeTypes.Created:
                            _logProvider.Information($"Adding scenario \"{change.Name}\"");
                            GetScenario(change.Name);
                            break;
                        case ScenarioChangeTypes.Deleted:
                            _logProvider.Information($"Deleting scenario \"{change.Name}\"");
                            DeleteScenario(change.Name);
                            break;
                        case ScenarioChangeTypes.Changed:
                            _logProvider.Information($"Updating scenario \"{change.Name}\"");
                            DeleteScenario(change.Name);
                            GetScenario(change.Name);
                            break;
                        case ScenarioChangeTypes.Renamed:
                            _logProvider.Information($"Renaming scenario \"{change.OldName}\" to  \"{change.Name}\"");
                            DeleteScenario(change.Name);
                            GetScenario(change.Name);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logProvider.Warning($"Error while handling scenario \"{change.Name}\" {change.ChangeType} change: {e.Message}");
            }
        }
        public MockScenario GetScenario(string scenarioName)
        {
            return _scenerioCache.GetOrAdd(scenarioName, (key) =>
            {
                return LoadScenario(scenarioName);
            });
        }
        public MockScenario FindScenario( string path )
        {
            lock(_lock)
            {
                foreach( var routeEntry in _routes)
                {
                    if ( routeEntry.Value.IsMatch(path))
                    {
                        return routeEntry.Value.Scenario;
                    }
                }
                return null;
            }
        }
        private MockScenario LoadScenario(string scenarioName)
        {
            var scenarioStream = _scenarioResolver.Resolve(scenarioName);
            if (scenarioStream == null)
            {
                _logProvider.Warning($"scenario \"{scenarioName}\" was not found.");
                return null;
            }
            else
            {
                try
                {
                    var scenarioReader = new StreamReader(scenarioStream);
                    var scenarioSource = scenarioReader.ReadToEnd();
                    var mockScenario = _deserializer.Deserialize<MockScenario>(scenarioSource);
                    mockScenario.Name = scenarioName; //Ensure scenario name is correct.
                    _routes.Add(new RouteKey(mockScenario),new Route(mockScenario));
                    return mockScenario;
                }
                catch( SyntaxErrorException e)
                {
                    var message = $"error parsing scenario \"{scenarioName}\": {e.Message}";
                    _logProvider.Error(message);
                    throw new System.Exception(message,e);
                }
                catch (Exception e)
                {
                    var message = $"error loading scenario \"{scenarioName}\": {e}";
                    _logProvider.Error(message);
                    throw new System.Exception(message,e);
                }
                finally
                {
                    scenarioStream.Dispose();
                }
            }
        }
        public T Deserialize<T>(string yaml, Type type)
        {
            return (T)_deserializer.Deserialize(yaml, type);
        }
        public object Deserialize(string yaml, Type type)
        {
            return _deserializer.Deserialize(yaml, type);
        }
        public string Serialize(object graph)
        {
            return _serializer.Serialize(graph);
        }
    }
}
