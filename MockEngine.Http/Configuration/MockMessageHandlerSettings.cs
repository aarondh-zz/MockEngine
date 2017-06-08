using MockEngine.Http.MediaTypeParsers;
using MockEngine.Resolvers;
using MockEngine.Utilities;
using System.Configuration;
using System;
using MockEngine.Configuration;
using System.Collections.Generic;
using MockEngine.Http.Interfaces;
using MockEngine.Interfaces;

namespace MockEngine.Http.Configuration
{
    public class MockMessageHandlerSettings : ConfigurationSection, IMockHandlerSettings
    {
        private static readonly string DefaultTypeResolver = typeof(DefaultTypeResolver).AssemblyQualifiedName;
        private static readonly string DefaultLogProvider = typeof(TraceLogProvider).AssemblyQualifiedName;
        private static readonly string DefaultScenarioResolver = typeof(ResourceScenarioResolver).AssemblyQualifiedName;
        public static IMockHandlerSettings Settings
        {
            get { return ConfigurationManager.GetSection("mockMessageHandlerSettings") as IMockHandlerSettings; }
        }
        [ConfigurationProperty("name")]
        public string Name { get { return this["name"] as string; } }

        [ConfigurationProperty("rootPath")]
        public string RootPath { get { return this["rootPath"] as string; } }
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled { get { return (bool)this["enabled"]; } }

        [ConfigurationProperty("when", IsRequired = true)]
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