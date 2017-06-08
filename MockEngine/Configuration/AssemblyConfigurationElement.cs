using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Configuration
{
    public class AssemblyConfigurationElement : ConfigurationElement, IMockAssemblySettings
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return base["name"] as string;
            }
        }


    }
}
