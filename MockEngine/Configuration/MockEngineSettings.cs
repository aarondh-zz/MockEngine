using MockEngine.Interfaces;
using System;
using System.Configuration;
namespace MockEngine.Configuration
{
    public class MockEngineSettings : ConfigurationSection, IMockEngineSettings
    {
        private static MockEngineSettings _sCacheSettings;

        public static MockEngineSettings Settings
        {
            get
            {
                return _sCacheSettings ??
                       (_sCacheSettings =
                        ConfigurationManager.GetSection("mockEngineSettings") as MockEngineSettings);
            }
        }

        [ConfigurationProperty("logProvider")]
        public string LogProvider
        {
            get
            {
                return this["logProvider"] as string;
            }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("rootPath")]
        public string RootPath
        {
            get
            {
                return this["rootPath"] as string;
            }
        }

        [ConfigurationProperty("scenarioResolver")]
        public string ScenarioResolver
        {
            get
            {
                return this["scenarioResolver"] as string;
            }
        }

        [ConfigurationProperty("typeResolver")]
        public string TypeResolver
        {
            get
            {
                return this["typeResolver"] as string;
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

