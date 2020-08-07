using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using UpDiddyApi.ApplicationCore.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column("IsEmailVerifiedLegacy")]
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
        public bool NotificationEmailsEnabled { get; set; }
        [MaxLength(100)]
        public string Auth0UserId { get; set; }
        public DateTime? LastSignIn { get; set; }
        public string Title { get; set; }
        public string Biography { get; set; }
        public string DreamJob { get; set; }
        public string CurrentRoleProficiencies { get; set; }
        public string PreferredLeaderStyle { get; set; }
        public string PreferredTeamType { get; set; }
        public string PassionProjectsDescription { get; set; }
        public string CoverLetter { get; set; }
        public Topic Topic { get; set; }
        public int? TopicId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesiredRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesiredSalary { get; set; }

        public virtual List<SubscriberLink> SubscriberLinks { get; set; }

        public List<SubscriberEmploymentTypes> SubscriberEmploymentTypes { get; set; } = new List<SubscriberEmploymentTypes>();

        public bool? IsFlexibleWorkScheduleRequired { get; set; }

        public bool? IsWillingToTravel { get; set; }

        public int? CommuteDistanceId { get; set; }
        public CommuteDistance CommuteDistance { get; set; }

        public bool? IsVideoVisibleToHiringManager { get; set; }

        public virtual List<SubscriberLanguageProficiency> SubscriberLanguageProficiencies { get; set; }

        #region Avatar Urls
        public string LinkedInAvatarUrl { get; set; }
        public string AvatarUrl { get; set; }

        #endregion

        #region Google Profile

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

        public int CloudTalentIndexVersion { get; set; }

        #endregion

        #region hubspot

        public long? HubSpotVid { get; set; }
        public DateTime? HubSpotModifyDate { get; set; }

        #endregion

        #region Connected
        public string  ConnectedId { get; set; }
        public DateTime? ConnectedModifyDate { get; set; }
        #endregion

        #region Computed Columns

        public Guid? StateGuid { get; set; }
        public Guid? CityGuid { get; set; }
        public Guid? PostalGuid { get; set; }

        #endregion


        #region Azure Index Info
        
        public int? AzureIndexStatusId { get; set; }
        public DateTime? AzureIndexModifyDate { get; set; }
        public string AzureSearchIndexInfo { get; set; }




        #endregion
    }
}