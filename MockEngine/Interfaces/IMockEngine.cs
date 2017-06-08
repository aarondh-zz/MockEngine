using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface IMockEngine
    {
        IMockEngineResponse Invoke(string scenario, object dynamicParameters = null);
        IScenario FindScenario(string path);
    }
}
