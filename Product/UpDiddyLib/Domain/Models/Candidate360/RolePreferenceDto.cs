using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class RolePreferenceDto
    {
        [JsonProperty("jobTitle")]
        public string JobTitle { get; set; }

        [JsonProperty("dreamJob")]
        public string DreamJob { get; set; }

        [JsonProperty("whatSetsMeApart")]
        public string WhatSetsMeApart { get; set; }

        [JsonProperty("whatKindOfLeader")]
        public string WhatKindOfLeader { get; set; }

        [JsonProperty("whatKindOfTeam")]
        public string WhatKindOfTeam { get; set; }

        [JsonProperty("volunteerOrPassionProjects")]
        public string VolunteerOrPassionProjects { get; set; }

        [JsonProperty("skillGuids")]
        public List<Guid> SkillGuids { get; set; } = new List<Guid>();

        [JsonProperty("socialLinks")]
        public List<SocialLinksDto> SocialLinks { get; set; } = new List<SocialLinksDto>();

        [JsonProperty("elevatorPitch")]
        public string ElevatorPitch { get; set; }
    }
}
