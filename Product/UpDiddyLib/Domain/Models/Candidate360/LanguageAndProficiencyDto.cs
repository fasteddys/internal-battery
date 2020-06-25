using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class LanguageAndProficiencyDto
    {
        public Guid LanguageGuid { get; set; }

        public Guid ProficiencyGuid { get; set; }
    }

    public class LanguageAndProficiencyListDto
    {
        public List<LanguageAndProficiencyDto> LanguagesAndProficiencies { get; set; } = new List<LanguageAndProficiencyDto>();

        public int TotalRecords => LanguagesAndProficiencies?.Count ?? 0;
    }
}