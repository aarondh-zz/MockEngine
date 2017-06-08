using MockEngine.Configuration;
using MockEngine.Http.Interfaces;
using MockEngine.Http.MediaTypeParsers;
using MockEngine.Resolvers;
using MockEngine.Utilities;
using System.Collections.Generic;
using System.Configuration;
using MockEngine.Interfaces;
using System;

namespace MockEngine.Http.Configuration
{
    public class MockHttpHandlerSettings : ConfigurationSection, IMockHandlerSettings
    {
        private const string DefaultBodyParameterName = "body";
        private static readonly string DefaultTypeResolver = typeof(DefaultTypeResolver).AssemblyQualifiedName;
        private static readonly string DefaultMediaTypeParser = typeof(DynamicXmlMediaTypeParser).AssemblyQualifiedName;
        private static readonly string DefaultLogProvider = typeof(TraceLogProvider).AssemblyQualifiedName;
        private static readonly string DefaultScenarioResolver = typeof(ResourceScenarioResolver).AssemblyQualifiedName;
        public static IMockHandlerSettings Settings
        {
            get { return ConfigurationManager.GetSection("mockHttpHandlerSettings") as IMockHandlerSettings; }
        }
        [ConfigurationProperty("name")]
        public string Name { get { return this["name"] as string; } }
        [ConfigurationProperty("rootPath")]
        public string RootPath { get { return this["rootPath"] as string; } }
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled { get { return (bool)this["enabled"]; } }

        [ConfigurationProperty("when")]
        public string When { get { return this["when"] as string; } }
        [ConfigurationProperty("logProvider")]
        public string LogProvider
        {
            get
            {
                var logProvider = this["logProvider"] as string;
                return string.IsNullOrEmpty(logProvider) ? DefaultLogProvider : logProvider;
            }
        }
        [ConfigurationProperty("typeResolver")]
        public string TypeResolver
        {
            get
            {
                var typeResolver = this["typeResolver"] as string;
                return string.IsNullOrEmpty(typeResolver) ? DefaultTypeResolver : typeResolver;
            }
        }
        [ConfigurationProperty("scenarioResolver")]
        public string ScenarioResolver
        {
            get
            {
                var scenarioResolver = this["scenarioResolver"] as string;
                return string.IsNullOrEmpty(scenarioResolver) ? DefaultScenarioResolver : scenarioResolver;
            }
        }
        [ConfigurationProperty("mediaTypeParser")]
        public string MediaTypeParser
        {
            get
            {
                var mediaTypeParser = this["mediaTypeParser"] as string;
                return string.IsNullOrEmpty(mediaTypeParser) ? DefaultMediaTypeParser : mediaTypeParser;
            }
        }

        [ConfigurationProperty("scenarioResolverSettings")]
        public SenarioResolverSettingConfigurationElement ScenarioResolverSettings
        {
            get
            {
                return this["scenarioResolverSettings"] as SenarioResolverSettingConfigurationElement;
            }
        }
        IScenarioResolverSettings IMockEngineSettings.ScenarioResolverSettings
        {
            get
            {
                return this.ScenarioResolverSettings;
            }
        }
    }
}