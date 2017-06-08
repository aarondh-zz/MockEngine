using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Utilities
{
    public class TraceLogProvider : ILogProvider
    {
        public void Verbose(string message)
        {
            Trace.WriteLine(message);
        }
        public void Information(string message)
        {
            Trace.TraceInformation(message);
        }
        public void Warning(string message)
        {
            Trace.TraceWarning(message);
        }
        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        public void Initialize(IMockContext context)
        {
        }
    }
}
