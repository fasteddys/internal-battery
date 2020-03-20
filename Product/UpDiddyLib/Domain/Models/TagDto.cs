using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class TagListDto
    {
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
        public int TotalRecords { get; set; }
    }

    public class TagDto
    {
        public Guid TagGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}