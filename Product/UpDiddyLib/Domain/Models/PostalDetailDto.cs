using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class PostalDetailListDto
    {
        public List<PostalDetailDto> Postals { get; set; } = new List<PostalDetailDto>();
        public int TotalRecords { get; set; }
    }

    public class PostalDetailDto
    {
        public Guid CityGuid { get; set; }
        public Guid PostalGuid { get; set; }
        public string Code { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
