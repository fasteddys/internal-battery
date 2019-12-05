using System;

namespace UpDiddyLib.Domain.Models
{
    public class TopicDto 
    {
        public Guid? TopicGuid;
        public string Name;
        public string Description;
        public int? SortOrder;
    }
}
