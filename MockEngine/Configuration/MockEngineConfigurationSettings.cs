using MockEngine.Interfaces;
using System;
using System.Configuration;
namespace MockEngine.Configuration
{
    public class MockEngineConfigurationSettings : ConfigurationSection, IMockEngineSettings
    {
        private static MockEngineConfigurationSettings _sCacheSettings;

        public static MockEngineConfigurationSettings Settings
        {
            get
            {
                return _sCacheSettings ??
                       (_sCacheSettings =
                        ConfigurationManager.GetSection("mockEngineSettings") as MockEngineConfigurationSettings);
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

