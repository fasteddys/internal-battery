using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class TagTopic : BaseModel
    {
        public int TagTopicId { get; set; }
        public Guid? TagTopicGuid { get; set; }
        public int TagId { get; set; }
        public int TopicId { get; set; }
    }
}
