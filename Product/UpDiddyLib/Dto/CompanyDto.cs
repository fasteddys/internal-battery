using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Dto
{
    public class CompanyListDto
    {
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();
        public int TotalRecords { get; set; }
    }

    public class CompanyDto 
    {
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        public string JobPageBoilerplate { get; set; }
        public string LogoUrl { get; set; }
        public int IsHiringAgency { get; set; }
        public int IsJobPoster { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}