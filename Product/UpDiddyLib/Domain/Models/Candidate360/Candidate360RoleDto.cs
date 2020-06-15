using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class Candidate360RoleDto
    {
        public Guid SubscriberGuid { get; set; }

        public string Title { get; set; }

        public string CurrentRoleProficiencies { get; set; }

        public string DreamJob { get; set; }

        public string PreferredLeaderStyle { get; set; }

        public string PreferredTeamType { get; set; }

        public string PassionProjects { get; set; }

        public string CoverLetter { get; set; }

        public List<SocialLinksDto> SocialLinks { get; set; }

        public List<SkillDto> Skills { get; set; }
    }
}
