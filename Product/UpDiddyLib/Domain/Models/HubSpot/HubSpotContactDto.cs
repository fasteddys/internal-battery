using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.HubSpot
{
    public class HubSpotContactDto
    {
   
        public Guid? SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string Email { get; set; }     
        public long? HubSpotVid { get; set; }
        public DateTime? DateJoined { get; set; }
        public DateTime? LastLoginDate  { get; set; }
        public DateTime? LastResumeUploadDate { get; set; }
        public string SelfCuratedSkills { get; set; }
        public string SkillsG2 { get; set; }
        public string SourcePartner { get; set; }
        public bool? IsHiringManager { get; set; }
    }
}
