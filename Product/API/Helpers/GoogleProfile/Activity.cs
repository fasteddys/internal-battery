using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Activity
    {
        string displayName { get; set; }
        string description { get; set; }
        string uri { get; set; }
        Date createDate { get; set; }
        Date updateDate { get; set; }
        List<string> teamMembers { get; set; } 
        List<Skill> skillsUsed { get; set; }
        string activityNameSnippet { get; set; }

        string activityDescriptionSnippet { get; set; }
        string skillsUsedSnippet { get; set; }

    }
}
