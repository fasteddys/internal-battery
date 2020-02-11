using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class HiringSolvedResumeParse : BaseModel
    {
        public int HiringSolvedResumeParseId { get; set; }
        public Guid HiringSolvedResumeParseGuid { get; set; }

        public DateTime? ParseRequested { get; set; }

        public DateTime? ParseCompleted { get; set; }

   
        public int SubscriberId { get; set; }

        public virtual Subscriber Subscriber { get; set; }

        public string ParseStatus { get; set; }

        public string JobId { get; set; }

        public string FileName { get; set; }

        public string ResumeText { get; set; }

        public string ParsedResume { get; set; }

        public long NumTicks { get; set; }

    }
}
