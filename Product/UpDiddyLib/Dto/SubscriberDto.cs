using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberDto : BaseDto
    {
        public int SubscriberId { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int? GenderId { get; set; }
        public int? EducationLevelId { get; set; }        
        public string ProfileImage { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string LinkedInUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string StackOverflowUrl { get; set; }
        public string GithubUrl { get; set; }
        public int HasOnboarded { get; set; }
        public StateDto State { get; set; }
        public List<EnrollmentDto> Enrollments { get; set; }
        public List<SkillDto> Skills { get; set; }
        public List<SubscriberWorkHistoryDto> WorkHistory { get; set; }
        public List<SubscriberEducationHistoryDto> EducationHistory { get; set; }
        public List<SubscriberFileDto> Files { get; set; }
        public List<SubscriberNotificationDto> SubscriberNotifications { get; set; }
        public List<CampaignDto> EligibleCampaigns { get; set; } = new List<CampaignDto>();

        // Marketing campaign offers that have not been used by subscriber
        public string CampaignOffer { get; set; }
        public bool HasVerificationEmail { get; set; }
        public DateTime? LinkedInSyncDate { get; set; }       
        public string LinkedInAvatarUrl { get; set; }
        public string AvatarUrl { get; set; }
        
    }
}
