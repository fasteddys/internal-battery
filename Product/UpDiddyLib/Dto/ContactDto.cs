using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ContactDto 
    {
        public string Email { get; set; }
        public string SourceSystemIdentifier { get; set; }
        public Dictionary<string,string> Metadata { get; set; }
    }
}
