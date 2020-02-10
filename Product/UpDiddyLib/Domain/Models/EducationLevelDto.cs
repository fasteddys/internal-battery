using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class EducationLevelListDto
    {
        public List<EducationLevelDto> EducationLevels { get; set; } = new List<EducationLevelDto>();
        public int TotalRecords { get; set; }
    }

    public class EducationLevelDto
    {
        public Guid EducationLevelGuid { get; set; }
        public string Level { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
