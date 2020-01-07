using System;
using Newtonsoft.Json;
namespace UpDiddyLib.Domain.Models
{
    public class CampaignLandingPage
    {
        public bool iswaitlist { get; set; }
        public bool isgateddownload { get; set; }
        public string gatedfiledownloadfile { get; set; }
        public decimal? gatedfiledownloadmaxattemptsallowed { get; set; }
        public PartnerCollection partner {get;set;}
    }

    public class PartnerCollection
    {

        [JsonProperty("name")]
        public string PartnerName { get; set; }
        [JsonProperty("guid")]
        public Guid PartnerGuid { get; set; }
        [JsonProperty("description")]
        public string PartnerDescription { get; set; }
    }
}


