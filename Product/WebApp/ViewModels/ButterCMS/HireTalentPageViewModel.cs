using System.Collections.Generic;
using Newtonsoft.Json;
using ButterCMS.Models;

namespace UpDiddy.ViewModels.ButterCMS
{
    public class HireTalentPageViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("hero_header")]
        public string Header { get; set; }
        [JsonProperty("hero_content")]
        public string Content { get; set; }
        [JsonProperty("hero_footer")]
        public string Footer { get; set; }
    }
}