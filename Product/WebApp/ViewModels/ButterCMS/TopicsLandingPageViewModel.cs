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
        [JsonProperty("hero_description")]
        public string HeroDescription { get; set; }
        [JsonProperty("training_vendors")]
        public IList<TrainingVendorViewModel> TrainingVendors { get; set; }
    }

    
}
