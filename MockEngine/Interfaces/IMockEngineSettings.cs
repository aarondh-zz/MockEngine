using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface IMockEngineSettings
    {
        string Name { get; }
        string RootPath { get; }
        string TypeResolver { get; }
        string LogProvider { get; }
        string ScenarioResolver { get; }
        IScenarioResolverSettings ScenarioResolverSettings { get; }
    }
}
