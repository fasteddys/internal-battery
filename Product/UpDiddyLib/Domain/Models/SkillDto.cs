using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class SkillListDto
    {
        public List<SkillDto> Skills { get; set; } = new List<SkillDto>();
        public int TotalRecords { get; set; }
    }

    public class SkillDto
    {
        public Guid SkillGuid { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}