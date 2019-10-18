using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{

    public class TrainingTopicViewModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("topic_title")]
        public string TopicTitle { get; set; }
        [JsonProperty("sortorder")]
        public double? SortOrder { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }


}
