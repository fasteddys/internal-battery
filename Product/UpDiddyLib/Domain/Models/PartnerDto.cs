using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models
{
    public class PartnerListDto
    {
        public List<PartnerDto> Partners { get; set; } = new List<PartnerDto>();
        public int TotalRecords { get; set; }
    }

    public class PartnerDto
    {
        public Guid? PartnerGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}