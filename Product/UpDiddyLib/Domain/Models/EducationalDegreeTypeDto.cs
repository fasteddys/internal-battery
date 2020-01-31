using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class EducationalDegreeTypeListDto
    {
        public List<EducationalDegreeTypeDto> EducationalDegreeTypes { get; set; } = new List<EducationalDegreeTypeDto>();
        public int TotalRecords { get; set; }
    }
    public class EducationalDegreeTypeDto
    {
        public Guid EducationalDegreeTypeGuid { get; set; }
        public string DegreeType  { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}