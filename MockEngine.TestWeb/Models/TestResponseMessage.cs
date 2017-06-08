using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MockEngine.TestWeb.Models
{
    [DataContract]
    public class TestResponseMessage
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public int ANumber { get; set; }
        [DataMember]
        public DateTime ADateTime { get; set; }
        [DataMember]
        public List<string> AList { get; set; }
        [DataMember]
        public Guid AGuid { get; set; }
        [DataMember]
        public ResponseEnumValues AnEnum { get; set; }
    }
}