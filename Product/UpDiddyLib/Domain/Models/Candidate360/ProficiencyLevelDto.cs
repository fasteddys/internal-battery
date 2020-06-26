using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class ProficiencyLevelDto
    {
        public Guid ProficiencyLevelGuid { get; set; }

        public string ProficiencyLevelName { get; set; }
    }

    public class ProficiencyLevelListDto
    {
        public List<ProficiencyLevelDto> Proficiencies { get; set; } = new List<ProficiencyLevelDto>();

        public int TotalRecords => Proficiencies?.Count ?? 0;
    }
}
