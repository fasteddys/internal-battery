using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class ExperienceLevelListDto
    {
        public List<ExperienceLevelDto> ExperienceLevels { get; set; } = new List<ExperienceLevelDto>();
        public int TotalRecords { get; set; }
    }

    public class ExperienceLevelDto
    {
        public Guid ExperienceLevelGuid { get; set; }
        public string DisplayName { get; set; }
        public string Code { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
