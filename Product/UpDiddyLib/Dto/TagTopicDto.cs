using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class TagTopicDto : BaseDto
    {
        public Guid TagTopicGuid { get; set; }
        public TagDto Tag { get; set; }
        public TopicDto Topic { get; set; }
    }
}
