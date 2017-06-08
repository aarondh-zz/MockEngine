using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface ILogProvider : IMockComponent
    {
        void Warning(string message);
        void Error(string message);
        void Information(string message);
        void Verbose(string message);
    }
}
