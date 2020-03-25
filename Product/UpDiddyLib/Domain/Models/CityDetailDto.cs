using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CityDetailListDto
    {
        public List<CityDetailDto> Cities { get; set; } = new List<CityDetailDto>();
        public int TotalRecords { get; set; }
    }

    public class CityDetailDto
    {   
        public Guid StateGuid { get; set; }
        public Guid CityGuid { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
