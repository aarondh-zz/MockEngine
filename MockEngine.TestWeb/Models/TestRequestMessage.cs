using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MockEngine.TestWeb.Models
{
    [DataContract]
    public class TestRequestMessage
    {
        [DataMember]
        public int ANumber { get; set; }
        [DataMember]
        public string AString { get; set; }
        [DataMember]
        public List<string> AList { get; set; }
    }
}