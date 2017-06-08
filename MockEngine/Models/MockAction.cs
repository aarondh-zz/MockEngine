using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace MockEngine.Models
{
    public class MockAction
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }
        [YamlMember(Alias = "description")]
        public string Description { get; set; }
        [YamlMember(Alias = "when")]
        public string When { get; set; }
        [YamlMember(Alias = "before")]
        public string Before { get; set; }
        [YamlMember(Alias = "response")]
        public MockMessage Response { get; set; }
        [YamlMember(Alias = "after")]
        public string After { get; set; }
        [YamlMember(Alias = "log")]
        public string Log { get; set; }

        public override string ToString()
        {
            if ( !string.IsNullOrWhiteSpace(Name) )
            {
                return Name;
            }
            else if ( !string.IsNullOrWhiteSpace(When) )
            {
                return When;
            }
            else
            {
                return "*";
            }
        }
    }
}
