using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.Components.Layout
{
    public class PublicSiteNavigationMenuItemViewModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("child_pages")]
        public IList<PublicSiteNavigationMenuItemViewModel> ChildPages { get; set; }
        [JsonProperty("role")]
        public RoleViewModel Role { get; set; }
        [JsonProperty("needs_authentication")]
        public bool NeedsAuthentication { get; set; }
        [JsonProperty("font_awesome_icon")]
        public string FontAwesomeIcon { get; set; }
        [JsonProperty("badge_text")]
        public string BadgeText { get; set; }

    }
}
