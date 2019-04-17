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
        public IList<TopicLandingViewModel> Topics { get; set; }
        [JsonProperty("hero_description")]
        public string HeroDescription { get; set; }
    }

    public class TopicLandingViewModel
    {
        [JsonProperty("topic_title")]
        public string TopicTitle { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
    }

    
}
