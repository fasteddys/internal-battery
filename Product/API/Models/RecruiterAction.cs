using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class RecruiterAction : BaseModel
    {
        public Guid RecruiterActionGuid { get; set; }
        public int RecruiterActionId { get; set; }
        public virtual Recruiter Recruiter { get; set; }
        public int RecruiterId { get; set; }
        public virtual Action Action { get; set; }
        public int ActionId { get; set; }
        public DateTime OccurredDate { get; set; }
        public int? EntityId { get; set; }
        public virtual EntityType EntityType { get; set; }
        public int? EntityTypeId { get; set; }
    }
}
