using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ResumeParse : BaseModel
    {
        public int ResumeParseId { get; set; }
        public Guid ResumeParseGuid { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }

        public int SubscriberFileId { get; set; }
        public virtual SubscriberFile SubscriberFile { get; set; }
        public int ParseStatus { get; set; }

        public int RequiresMerge { get; set; }

    }
}
