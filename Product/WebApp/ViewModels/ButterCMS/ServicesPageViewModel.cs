using System.Collections.Generic;
using Newtonsoft.Json;
using ButterCMS.Models;

namespace UpDiddy.ViewModels.ButterCMS
{
    public class ServicesPageViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("hero_title")]
        public string HeroTitle { get; set; }
        [JsonProperty("hero_content")]
        public string HeroContent { get; set; }
        [JsonProperty("hero_image")]
        public string HeroImage { get; set; }
        [JsonProperty("packages")]
        public IList<Page<PackageServiceViewModel>> PackagesFromCms { get; set; }
        public IList<PackageServiceViewModel> Packages { get; set; }
    }
}