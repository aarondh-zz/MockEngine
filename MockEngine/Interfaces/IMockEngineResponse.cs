using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Interfaces
{
    public interface IMockEngineResponse
    {
        bool Success { get; }
        HttpStatusCode StatusCode { get; }
        string ReasonPhrase { get; }
        object Content { get; }
        IDictionary<string, string> Headers { get; }
    }
}
