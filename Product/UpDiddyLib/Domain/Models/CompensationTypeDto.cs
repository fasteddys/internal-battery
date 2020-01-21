using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class CompensationTypeListDto
    {
        public List<CompensationTypeDto> CompensationTypes { get; set; } = new List<CompensationTypeDto>();
        public int TotalRecords { get; set; }
    }

    public class CompensationTypeDto
    {
        public Guid CompensationTypeGuid { get; set; }
        public string CompensationTypeName { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}