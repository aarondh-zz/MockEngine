using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Configuration
{
    public class MockEngineSettings : IMockEngineSettings
    {
        public MockEngineSettings()
        {
            this.ScenarioResolverSettings = new ScenarioResolverSettings();
            var codebase = Assembly.GetCallingAssembly().EscapedCodeBase;
            this.RootPath = System.IO.Path.GetDirectoryName(new Uri(codebase).LocalPath);
        }
        public string LogProvider { get; set; }
        public string Name { get; set; }
        public string RootPath { get; set; }
        public string ScenarioResolver { get; set; }
        public ScenarioResolverSettings ScenarioResolverSettings { get; set; }
        public string TypeResolver { get; set; }


        IScenarioResolverSettings IMockEngineSettings.ScenarioResolverSettings
        {
            get
            {
                return this.ScenarioResolverSettings;
            }
        }
    }
}
