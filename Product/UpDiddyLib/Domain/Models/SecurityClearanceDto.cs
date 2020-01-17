using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class SecurityClearanceListDto
    {
        public List<SecurityClearanceDto> SecurityClearances { get; set; } = new List<SecurityClearanceDto>();
        public int TotalRecords { get; set; }
    }

    public class SecurityClearanceDto
    {
        public Guid SecurityClearanceGuid { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
