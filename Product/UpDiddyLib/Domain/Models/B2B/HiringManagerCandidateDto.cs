using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class HiringManagerCandidateDto
    {
        [JsonProperty("@search.score")]
        public double SearchScore { get; set; }

        public Guid ProfileGuid { get; set; }

        public string FirstName { get; set; }

        public string PersonalityBlendName { get; set; }

        public Uri Personality1ImageUrl { get; set; }

        public Uri Personality2ImageUrl { get; set; }

        public Uri VideoUrl { get; set; }

        public Uri ThumbnailImageUrl { get; set; }

        public string Title { get; set; }

        public string Skills { get; set; }

        public string ExperienceSummary { get; set; }

        public DateTime? lastContactDate { get; set; }
    }
}
