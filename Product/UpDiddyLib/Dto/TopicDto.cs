using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Dto
{
    public class TopicDto
    {
        public int TopicId { get; set; }
        public Guid? TopicGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DesktopImage { get; set; }
        public string TabletImage { get; set; }
        public string MobileImage { get; set; }
        public int? SortOrder { get; set; }

    }
}
