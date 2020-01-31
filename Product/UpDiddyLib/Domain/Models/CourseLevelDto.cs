using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class CourseLevelListDto
    {
        public List<CourseLevelDto> CourseLevels { get; set; } = new List<CourseLevelDto>();
        public int TotalRecords { get; set; }
    }

    public class CourseLevelDto
    {
        public Guid CourseLevelGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}