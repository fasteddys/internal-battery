using Newtonsoft.Json;

namespace UpDiddy.ViewModels.ButterCMS
{
    public class ServicesPageViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("hero_title")]
        public string HeroTitle { get; set; }
        [JsonProperty("hero_content")]
        public string HeroContent { get; set; }
    }
}