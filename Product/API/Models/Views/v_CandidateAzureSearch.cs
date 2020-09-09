using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_CandidateAzureSearch
    {
        public string AvatarUrl { get; set; }

        public int? AzureIndexStatusId { get; set; }

        public string City { get; set; }

        public string CommuteDistance { get; set; }

        public DateTime? CreateDate { get; set; }

        public double? CurrentRate { get; set; }

        public double? DesiredRate { get; set; }

        public string Education { get; set; }

        public string Email { get; set; }

        public string EmploymentTypes { get; set; }

        public string ExperienceSummary { get; set; }

        public string FirstName { get; set; }

        public bool? IsResumeUploaded { get; set; }

        public string Languages { get; set; }

        public DateTime? LastCertifiedDate { get; set; }

        public DateTime? LastContactDate { get; set; }

        public string LastName { get; set; }

        public SqlGeography Location { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string Personalities { get; set; }

        public string Personality1ImageUrl { get; set; }

        public string Personality2ImageUrl { get; set; }

        public string PersonalityBlendName { get; set; }

        public string PhoneNumber { get; set; }

        public string Postal { get; set; }

        public Guid? ProfileGuid { get; set; }

        public string Skills { get; set; }

        public string State { get; set; }

        public string StreetAddress { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string ThumbnailImageUrl { get; set; }

        public string Title { get; set; }

        public string Training { get; set; }

        public string VideoUrl { get; set; }

        public string WorkHistories { get; set; }

        public string WorkPreferences { get; set; }
    }
}
