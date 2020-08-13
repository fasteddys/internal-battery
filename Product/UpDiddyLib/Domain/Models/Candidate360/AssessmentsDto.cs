using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class AssessmentsDto
    {
        public TraitifyDto Traitify { get; set; }
        public bool IsTraitifyAssessmentsVisibleToHiringManagers { get; set; }
    }

    public class TraitifyDto
    {
        public Guid TraitifyId { get; set; }
        public string DeckId { get; set; }
        public string Status { get; set; }
        public string PersonalityBlendName { get; set; }
        public string Personality1ImageUrl { get; set; }
        public string Personality2ImageUrl { get; set; }
        public string Description { get; set; }
        public string PublicKey { get; set; }
        public string HostUrl { get; set; }
    }
}
