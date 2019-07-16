using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.ButterCMS
{
    public class PartnerViewModel
    {

        [JsonProperty("name")]
        public string PartnerName { get; set; }
        [JsonProperty("guid")]
        public Guid PartnerGuid { get; set; }
        [JsonProperty("description")]
        public string PartnerDescription { get; set; }
    }
}
