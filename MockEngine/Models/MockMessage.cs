using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockEngine.Models
{
    public class MockMessage
    {
        [YamlMember(Alias = "statusCode")]
        public string StatusCode { get; set; }
        [YamlMember(Alias = "contentType")]
        public string ContentType { get; set; }
        [YamlMember(Alias = "reason")]
        public string Reason { get; set; }
        [YamlMember(Alias ="bodyType")]
        public string BodyType { get; set; }
        [YamlMember(Alias = "body")]
        public dynamic Body { get; set; }
        [YamlMember(Alias = "bodyXml")]
        public string BodyXml { get; set; }
    }
}
