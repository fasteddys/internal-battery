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

        public string Personality1ImageUrl { get; set; }

        public string Personality2ImageUrl { get; set; }

        public string VideoUrl { get; set; }

        public string ThumbnailImageUrl { get; set; }

        public string Title { get; set; }

        public ICollection<string> Skills { get; set; } = new List<string>();

        public string ExperienceSummary { get; set; }

        public DateTime? LastContactDate { get; set; }
    }
}
