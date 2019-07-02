using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Skill
    {
        public Skill() { }
        public string displayName { get; set;  }

        public Skill(SubscriberSkill subscriberSkill)
        {
            this.displayName = subscriberSkill.Skill?.SkillName;
        }
    }
}
