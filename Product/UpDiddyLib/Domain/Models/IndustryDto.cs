using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class IndustryListDto
    {
        public List<IndustryDto> Industries { get; set; } = new List<IndustryDto>();
        public int TotalRecords { get; set; }
    }

    public class IndustryDto
    {
        public Guid IndustryGuid { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
