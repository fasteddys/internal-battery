using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class ProficiencyDto
    {
        public Guid ProficiencyGuid { get; set; }

        public Guid ProficiencyName { get; set; }
    }

    public class ProficiencyListDto
    {
        public List<ProficiencyDto> Proficiencies { get; set; } = new List<ProficiencyDto>();

        public int TotalRecords => Proficiencies?.Count ?? 0;
    }
}
