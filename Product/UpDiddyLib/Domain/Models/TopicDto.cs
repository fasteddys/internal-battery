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
        public Guid? TopicGuid;
        public string Name;
        public string Description;
        public int? SortOrder;
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
