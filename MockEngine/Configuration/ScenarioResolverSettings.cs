using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Configuration
{
    public class ScenarioResolverSettings : IScenarioResolverSettings
    {
        public ScenarioResolverSettings()
        {
            this.PathBase = "~/Scenarios";
            this.PathSuffix = ".yaml";
            AddAssembly(GetType().Assembly);
        }
        private class MockAssemblySettings : IMockAssemblySettings
        {
            public string Name { get; set; }
        }
        public List<IMockAssemblySettings> _assemblies;
        public void AddAssembly( Assembly assembly )
        {
            AddAssembly(assembly.GetName().FullName);
        }
        public void AddAssembly( string fullName )
        {
            if (_assemblies == null)
            {
                _assemblies = new List<IMockAssemblySettings>();
            }
            _assemblies.Add(new MockAssemblySettings() { Name = fullName });
        }
        public IEnumerable<IMockAssemblySettings> Assemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    return new IMockAssemblySettings[] { };
                }
                return _assemblies;
            }
        }

        public string PathBase { get; set; }

        public string PathSuffix { get; set; }
    }
}
