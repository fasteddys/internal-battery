using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{
    
    public class TopicsLandingPageViewModel
    {
        [JsonProperty("hero_header")]
        public string HeroHeader { get; set; }
        [JsonProperty("hero_image")]
        public string HeroImage { get; set; }
        [JsonProperty("topics_vendor_logo")]
        public string TopicsVendorLogo { get; set; }
        [JsonProperty("topics_listing")]
        public IList<TopicViewModel> Topics { get; set; }
    }

    
}
