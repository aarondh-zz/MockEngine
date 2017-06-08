using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MockEngine.TestWeb.Models
{
    [DataContract(Name = "error")]
    public class TestErrorMessage
    {
        [DataMember(Name = "errorCode")]
        public string ErrorCode { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }

    }
}