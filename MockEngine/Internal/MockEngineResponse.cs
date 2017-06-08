using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockEngine.Internal
{
    internal class MockEngineResponse : IMockEngineResponse
    {
        private Dictionary<string, string> _headers;
        public MockEngineResponse()
        {
            StatusCode = HttpStatusCode.OK;
        }
        public object Content { get; set; }
        public IDictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = new Dictionary<string, string>();
                }
                return _headers;
            }
        }
        public string ReasonPhrase { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// If true a engine scenario was executed successfully
        /// </summary>
        public bool Success { get; set; }
    }
}
