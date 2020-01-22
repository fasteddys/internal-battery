using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class CountryDetailListDto
    {
        public List<CountryDetailDto> Countries { get; set; } = new List<CountryDetailDto>();
        public int TotalRecords { get; set; }
    }

    public class CountryDetailDto
    {
        public Guid CountryGuid {get;set;}
        public string DisplayName { get; set; }
        public string OfficialName { get; set; }
        public int Sequence { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}