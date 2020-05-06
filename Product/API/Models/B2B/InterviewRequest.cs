using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.Models.B2B
{
    public class InterviewRequest : BaseModel
    {
        public int InterviewRequestId { get; set; }

        public Guid InterviewRequestGuid { get; set; }

        public int HiringManagerId { get; set; }

        public virtual HiringManager HiringManager { get; set; }

        public int ProfileId { get; set; }

        public virtual Profile Profile { get; set; }

        public DateTime DateRequested { get; set; }
    }
}
