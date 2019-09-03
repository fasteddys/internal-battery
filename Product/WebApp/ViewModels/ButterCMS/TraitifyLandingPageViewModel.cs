using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{

    public class TraitifyLandingPageViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("hero_header")]
        public string HeroHeader { get; set; }
        [JsonProperty("hero_image")]
        public string HeroImage { get; set; }
        [JsonProperty("hero_description")]
        public string HeroDescription { get; set; }
        [JsonProperty("modal_header")]
        public string ModalHeader { get; set; }
        [JsonProperty("modal_text")]
        public string ModalText { get; set; }

        [JsonProperty("form_header")]
        public string FormHeader { get; set; }
        [JsonProperty("form_text")]
        public string FormText { get; set; }
        [JsonProperty("form_submit_button_text")]
        public string FormSubmitButtonText { get; set; }
    }
}
