using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public enum ScenarioChangeTypes
    {
        Changed,
        Created,
        Deleted,
        Renamed
    }
    public interface IScenarioChange
    {
        string Name { get; }
        string OldName { get; }
        ScenarioChangeTypes ChangeType { get; }
    }
    public interface IScenarioResolver : IMockComponent
    {
        Stream Resolve(string scenarioName);
        IEnumerable<string> GetScenarioNames();
        void RegisterChangeHandler(Action<IScenarioChange> onChange);
    }
}
