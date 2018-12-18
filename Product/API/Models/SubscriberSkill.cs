using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial  class SubscriberSkill : BaseModel
    {
        public int SubscriberSkillId { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
        public Guid? SubscriberSkillGuid { get; set; }
    }
}
