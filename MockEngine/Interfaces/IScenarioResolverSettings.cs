using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface IScenarioResolverSettings
    {
        string PathBase { get; }
        string PathSuffix { get; }
        IEnumerable<IMockAssemblySettings> Assemblies { get; }
    }
}
