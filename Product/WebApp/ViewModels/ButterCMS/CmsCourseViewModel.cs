using Newtonsoft.Json;

namespace UpDiddy.ViewModels.ButterCMS
{
    public class CmsCourseViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("hero_image")]
        public string Band1ImagePath { get; set; }
        [JsonProperty("band_2_title")]
        public string Band2Title { get; set;}
        [JsonProperty("band_2_text")]
        public string Band2Text { get; set; }
        [JsonProperty("band_3_left_text")]
        public string Band3LeftText { get; set; }
        [JsonProperty("band_3_right_text")]
        public string Band3RightText { get; set; }
        [JsonProperty("band_4_title")]
        public string Band4Title { get; set; }
        [JsonProperty("band_4_text")]
        public string Band4Text { get; set; }
        [JsonProperty("band_4_button_url")]
        public string Band4ButtonUrl { get; set; }
        [JsonProperty("band_4_button_text")]
        public string Band4ButtonText { get; set; }
    }
}