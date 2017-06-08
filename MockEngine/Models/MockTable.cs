using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MockEngine.Models
{
    public class MockTable
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }
        [YamlMember(Alias = "description")]
        public string Description { get; set; }
        [YamlMember(Alias = "rows")]
        public List<dynamic> Rows { get; set; }
    }
}
