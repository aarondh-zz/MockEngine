namespace MockEngine
{
    using System;
    using MockEngine.Resolvers;
    using MockEngine.Internal;
    using System.Reflection;
    using Utilities;
    using System.Collections.Generic;
    using Interfaces;

    public class MockEngineFactory : IMockContext
    {
        private IMockEngineSettings _settings;
        public MockEngineFactory()
        {
        }
        IMockEngineSettings IMockContext.Settings
        {
            get
            {
                return _settings;
            }
        }
        public void Initialize(IMockEngineSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            _settings = settings;
            this.LogProvider = new TraceLogProvider();
            this.LogProvider.Initialize(this);

            if (string.IsNullOrWhiteSpace(settings.RootPath))
            {
                var codebase = Assembly.GetCallingAssembly().EscapedCodeBase;
                RootPath = System.IO.Path.GetDirectoryName(new Uri(codebase).LocalPath);
            }
            else
            {
                RootPath = _settings.RootPath;
            }
            if (string.IsNullOrWhiteSpace(settings.TypeResolver))
            {
                this.TypeResolver = new DefaultTypeResolver();
                this.TypeResolver.Initialize(this);
            }
            else
            {
                this.TypeResolver = CreateComponent<ITypeResolver>(settings.TypeResolver);
            }
            if (!string.IsNullOrWhiteSpace(settings.LogProvider))
            {
                this.LogProvider = CreateComponent<ILogProvider>(settings.LogProvider);
            }
            if (string.IsNullOrWhiteSpace(settings.ScenarioResolver))
            {
                this.ScenarioResolver = new FileScenarioResolver();
                this.ScenarioResolver.Initialize(this);
            }
            else
            {
                this.ScenarioResolver = CreateComponent<IScenarioResolver>(settings.ScenarioResolver);
            }
        }
        public string RootPath { get; private set; }
        public T CreateComponent<T>(string assemblyQualifiedName) where T : IMockComponent
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
            {
                throw new ArgumentNullException(nameof(assemblyQualifiedName));
            }
            else
            {
                Type type = this.TypeResolver.Resolve(assemblyQualifiedName, true);
                if (typeof(T).IsAssignableFrom(type))
                {
                    T mockComponent = (T)Activator.CreateInstance(type);
                    mockComponent.Initialize(this);
                    return mockComponent;
                }
                else
                {
                    throw new Exception($"Type {type.FullName} does not implement {typeof(T).FullName}");
                }
            }
        }

        public IMockEngine CreateMockEngine(string engineName)
        {
            if (engineName == null)
            {
                throw new ArgumentNullException(nameof(engineName));
            }
            if (ScenarioResolver == null)
            {
                throw new InvalidOperationException("ScenarioResolver was not specified");
            }
            if (TypeResolver == null)
            {
                throw new InvalidOperationException("TypeResolver was not specified");
            }
            if (LogProvider == null)
            {
                throw new InvalidOperationException("LogProvider was not specified");
            }
            var mockEngine = new MockEngine(this, engineName, this.ScenarioResolver);
            
            return mockEngine;
        }
        public IScenarioResolver ScenarioResolver { get; set; }
        public ITypeResolver TypeResolver { get; set; }
        public ILogProvider LogProvider { get; set; }
    }
}
