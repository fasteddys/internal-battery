using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class LanguageProficiencyDto
    {
        public Guid LanguageGuid { get; set; }

        public string Language { get; set; }

        public Guid ProficiencyLevelGuid { get; set; }

        public string Level { get; set; }
    }

    public class LanguageProficiencyListDto
    {
        public List<LanguageProficiencyDto> LanguagesAndProficiencies { get; set; } = new List<LanguageProficiencyDto>();

        public int TotalRecords => LanguagesAndProficiencies?.Count ?? 0;
    }
}