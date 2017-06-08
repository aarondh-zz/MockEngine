using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine
{
    public class MockEngineException : Exception
    {
        public MockEngineException()
            : base()
        {

        }
        public MockEngineException(string engineName, string scenerioName)
            : base()
        {

        }
        public MockEngineException(string engineName, string scenerioName, string message) 
            : base(message)
        {

        }
        public MockEngineException(string engineName, string scenerioName, string message, Exception innerException) 
            : base(message, innerException)
        {

        }
    }
}
