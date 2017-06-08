using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Configuration
{
    public class SenarioResolverSettingConfigurationElement : ConfigurationElement, IScenarioResolverSettings
    {
        [ConfigurationProperty("pathBase")]
        public string PathBase
        {
            get
            {
                return this["pathBase"] as string;
            }
        }

        [ConfigurationProperty("pathSuffix")]
        public string PathSuffix
        {
            get
            {
                return this["pathSuffix"] as string;
            }
        }
        [ConfigurationProperty("assemblies", IsRequired = true)]
        [ConfigurationCollection(typeof(AssemblyConfigurationElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public GenericConfigurationElementCollection<AssemblyConfigurationElement> Assemblies
        {
            get { return (GenericConfigurationElementCollection<AssemblyConfigurationElement>)this["assemblies"]; }
        }

        IEnumerable<IMockAssemblySettings> IScenarioResolverSettings.Assemblies
        {
            get
            {
                return this.Assemblies;
            }
        }
    }
}
