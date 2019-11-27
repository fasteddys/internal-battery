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

        [JsonProperty("contact_form_header")]
        public string ContactFormHeader { get; set; }

        [JsonProperty("contact_form_text")]
        public string ContactFormText { get; set; }
    }
}