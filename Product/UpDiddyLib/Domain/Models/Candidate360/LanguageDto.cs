using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class LanguageDto
    {
        public Guid LanguageGuid { get; set; }

        public string LanguageName { get; set; }
    }

    public class LanguageListDto
    {
        public List<LanguageDto> Languages { get; set; } = new List<LanguageDto>();

        public int TotalRecords => Languages?.Count ?? 0;
    }
}
