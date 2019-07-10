using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Models
{
    public class Subscriber : BaseModel
    {
        public int SubscriberId { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int? GenderId { get; set; }
        public int? EducationLevelId { get; set; }
        public string ProfileImage { get; set; }
        public string City { get; set; }
        public int? StateId { get; set; }
        public virtual State State { get; set; }
        public string PostalCode { get; set; }
        public string LinkedInUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string StackOverflowUrl { get; set; }
        public string GithubUrl { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public int HasOnboarded { get; set; }
        public List<SubscriberSkill> SubscriberSkills { get; } = new List<SubscriberSkill>();
        public List<SubscriberEducationHistory> SubscriberEducationHistory { get; } = new List<SubscriberEducationHistory>();
        public List<SubscriberWorkHistory> SubscriberWorkHistory { get; } = new List<SubscriberWorkHistory>();
        public List<SubscriberFile> SubscriberFile { get; set; } = new List<SubscriberFile>();
        public EmailVerification EmailVerification {get; set;}
        public List<SubscriberProfileStagingStore> ProfileStagingStore { get; set; } = new List<SubscriberProfileStagingStore>();
        public DateTime? LinkedInSyncDate { get; set; }
        public List<SubscriberNotification> SubscriberNotifications { get; } = new List<SubscriberNotification>();

        #region Avatar Urls
        public string LinkedInAvatarUrl { get; set; }
        public string AvatarUrl { get; set; }

        /****************  google talent cloud information   ****************************/
        /// <summary>
        /// The uri returned from google talent cloud for identifying the posting in the
        /// talent cloud
        /// </summary>
        public string CloudTalentUri { get; set; }
        /// <summary>
        ///  The postings google cloud indexing status, see enum JobPostingIndexStatus
        /// </summary>
        public int CloudTalentIndexStatus { get; set; }
        /// <summary>
        /// Additional information such as error received from cloud talent
        /// </summary>
        public string CloudTalentIndexInfo { get; set; }
        #endregion
    }
}