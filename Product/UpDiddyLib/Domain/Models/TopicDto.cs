using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class TopicListDto
    {
        public List<TopicDto> Topics { get; set; } = new List<TopicDto>();
        public int TotalRecords { get; set; }
    }
    public class TopicDto 
    {
        public Guid? TopicGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
