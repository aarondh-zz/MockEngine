using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface IScenario
    {
        string Name { get; }
        string Description { get; }
        Type RequestType { get; }
        string RequestParameterName { get; }
    }
}
