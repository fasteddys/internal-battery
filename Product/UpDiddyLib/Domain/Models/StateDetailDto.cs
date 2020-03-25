using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class StateDetailListDto
    {
        public List<StateDetailDto> States { get; set; } = new List<StateDetailDto>();
        public int TotalRecords { get; set; }
    }

    public class StateDetailDto
    {
        public Guid CountryGuid { get; set; }
        public Guid StateGuid { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Sequence { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}