using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{
    public class ButterCMSBaseViewModel
    {
        [JsonProperty("meta_title")]
        public string MetaTitle { get; set; }
        [JsonProperty("meta_description")]
        public string MetaDescription { get; set; }
        [JsonProperty("meta_keywords")]
        public string MetaKeywords { get; set; }
        [JsonProperty("ogtitle")]
        public string OpenGraphTitle { get; set; }
        [JsonProperty("ogdescription")]
        public string OpenGraphDescription { get; set; }
        [JsonProperty("ogimage")]
        public string OpenGraphImage { get; set; }
    }
}
