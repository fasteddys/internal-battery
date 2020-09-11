using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry; 

namespace UpDiddyLib.Domain.AzureSearchDocuments
{
   public class CandidateSDOC
    {
        [JsonProperty("@search.action")]
        public string SearchAction { get; set; }

        public string AvatarUrl { get; set; }

        public string City { get; set; }

        public string CommuteDistance { get; set; }

        public DateTime? CreateDate { get; set; }

        public float? CurrentRate { get; set; }

        public float? DesiredRate { get; set; }

        public List<EducationSDOC> Education { get; set; }

        public string Email { get; set; }

        public string ExperienceSummary { get; set; }

        public string FirstName { get; set; }

        public bool? HasVideoInterview { get; set; }

        public bool? IsResumeUploaded { get; set; }

        public List<LanguageSDOC> Languages { get; set; }

        public DateTime? LastCertifiedDate { get; set; }

        public DateTime? LastContactDate { get; set; }

        public string LastName { get; set; }

        public Point Location { get; set; }

        public DateTime? ModifyDate { get; set; }

        public List<string> Personalities { get; set; }

        public string Personality1ImageUrl { get; set; }

        public string Personality2ImageUrl { get; set; }

        public string PersonalityBlendName { get; set; }

        public string PhoneNumber { get; set; }

        public string Postal { get; set; }

        public Guid? ProfileGuid { get; set; }

        public List<string> Skills { get; set; }

        public string State { get; set; }

        public string StreetAddress { get; set; }

        public string ThumbnailImageUrl { get; set; }

        public string Title { get; set; }

        public List<TrainingSDOC> Training { get; set; }

        public string VideoUrl { get; set; }

        public List<WorkHistorySDOC> WorkHistories { get; set; }

        public List<string> WorkPreferences { get; set; }
    }
}
