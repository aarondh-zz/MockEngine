using MockEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockEngine.Models
{
    public class MockScenario
    {
        [YamlMember(Alias = "path")]
        public string Path { get; set; }
        [YamlMember(Alias = "priority")]
        public float Priority { get; set; }
        [YamlMember(Alias = "name")]
        public string Name { get; set; }
        [YamlMember(Alias = "description")]
        public string Description { get; set; }
        [YamlMember(Alias = "request")]
        public MockRequest Request { get; set; }
        [YamlMember(Alias = "global")]
        public MockGlobal Global { get; set; }
        [YamlMember(Alias = "actions")]
        public List<MockAction> Actions { get; set; }
    }
}
