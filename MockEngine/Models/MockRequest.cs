using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockEngine.Models
{
    public class MockRequest
    {
        public MockRequest()
        {
            this.ParameterName = "body";
        }
        [YamlMember(Alias = "method")]
        public string Method { get; set; }
        [YamlMember(Alias = "path")]
        public string Path { get; set; }
        [YamlMember(Alias = "description")]
        public string Description { get; set; }
        [YamlMember(Alias = "type")]
        public string Type { get; set; }
        [YamlMember(Alias = "parameterName")]
        public string ParameterName { get; set; }
    }
}
