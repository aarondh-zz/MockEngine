using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockEngine.Models
{
    public class MockGlobal
    {
        [YamlMember(Alias = "before")]
        public string Before { get; set; }
        [YamlMember(Alias = "after")]
        public string After { get; set; }
        [YamlMember(Alias = "tables")]
        public List<MockTable> Tables { get; set; }
    }
}
