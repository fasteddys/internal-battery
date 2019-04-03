using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadStatusDto
    {
        public string Message { get; set; }
        [JsonIgnore]
        public string Name { get; set; }
        public string Severity { get; set; }
        [JsonIgnore]
        public int LeadStatusId { get; set; }
    }
}
