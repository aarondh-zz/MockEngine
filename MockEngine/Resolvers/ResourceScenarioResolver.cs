using MockEngine.Configuration;
using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Resolvers
{
    public class ResourceScenarioResolver : IScenarioResolver
    {
        private ILogProvider _logProvider;
        private HashSet<Assembly> _assemblies;
        private string _resourcePathBase;
        private string _resourcePathSuffix;
        public ResourceScenarioResolver( )
        {
            _resourcePathBase = "";

            _resourcePathSuffix = ".yaml";

            _assemblies = new HashSet<Assembly>();

            RegisterAssembly(Assembly.GetCallingAssembly());
        }
        public void Initialize( IMockContext context )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Settings == null)
            {
                throw new ArgumentException("Settings are not null", nameof(context));
            }
            if (context.Settings.ScenarioResolverSettings == null)
            {
                throw new ArgumentException("Settings.ScenarioResolverSettings are not null", nameof(context));
            }
            this._logProvider = context.LogProvider;
            if (!string.IsNullOrEmpty(context.Settings.ScenarioResolverSettings.PathBase))
            {
                _resourcePathBase = context.Settings.ScenarioResolverSettings.PathBase;
            }
            if (!string.IsNullOrEmpty(context.Settings.ScenarioResolverSettings.PathSuffix))
            {
                _resourcePathSuffix = context.Settings.ScenarioResolverSettings.PathSuffix;
            }
            foreach( var assembly in context.Settings.ScenarioResolverSettings.Assemblies)
            {
                RegisterAssembly(Assembly.Load(assembly.Name));
            }
        }
        public void RegisterAssembly( Assembly assembly)
        {
            _assemblies.Add(assembly);
        }
        public Stream Resolve(string scenarioName)
        {
            var resourcePath = _resourcePathBase + scenarioName + _resourcePathSuffix;
            Stream scenarioStream;
            foreach (var assembly in _assemblies)
            {
                scenarioStream = assembly.GetManifestResourceStream(resourcePath);
                if ( scenarioStream != null)
                {
                    return scenarioStream;
                }
            }
            _logProvider.Warning("scenario {resourcePath} was not found in any registered assembly", resourcePath);
            var names = new List<string>();
            foreach (var assembly in _assemblies)
            {
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    names.Add(resourceName);
                }
            }
            _logProvider.Verbose("valid resource names are: {names}", names);
            return null;
        }

        public IEnumerable<string> GetScenarioNames()
        {
            List<string> scenarioNames = new List<string>();
            foreach (var assembly in _assemblies)
            {
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    if ( resourceName.StartsWith(_resourcePathBase)&&resourceName.EndsWith(_resourcePathSuffix))
                    {
                        var scenarioName = resourceName.Substring(_resourcePathBase.Length, resourceName.Length - _resourcePathBase.Length - _resourcePathSuffix.Length);
                        scenarioNames.Add(scenarioName);
                    }
                    
                }
            }
            return scenarioNames;
        }

        public void RegisterChangeHandler(Action<IScenarioChange> onChange)
        {
        }
    }
}
