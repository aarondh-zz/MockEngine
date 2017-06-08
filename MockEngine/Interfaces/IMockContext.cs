using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface IMockContext
    {
        string RootPath { get; }
        IMockEngineSettings Settings { get; }
        ILogProvider LogProvider { get; }
        ITypeResolver TypeResolver { get; }
        T CreateComponent<T>(string assemblyQualifiedTypeString) where T : IMockComponent;
    }
}
