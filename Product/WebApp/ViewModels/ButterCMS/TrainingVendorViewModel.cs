using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{

    public class TrainingVendorViewModel
    {
        [JsonProperty("vendor_name")]
        public string VendorName { get; set; }
        [JsonProperty("vendor_logo")]
        public string VendorLogo { get; set; }
        [JsonProperty("vendor_guid")]
        public string VendorGuid { get; set; }
        [JsonProperty("training_topics_list")]
        public IList<TrainingTopicViewModel> TrainingTopicsList { get; set; }
    }


}
