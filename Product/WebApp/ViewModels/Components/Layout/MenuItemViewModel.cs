using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.Components.Layout
{
    public class MenuItemViewModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("child_pages")]
        public IList<MenuItemViewModel> ChildPages { get; set; }
    }
}
