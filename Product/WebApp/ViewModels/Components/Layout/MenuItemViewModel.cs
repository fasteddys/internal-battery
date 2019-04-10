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
        [JsonProperty("needs_admin_permissions")]
        public bool NeedsAdminPermissions { get; set; }
        [JsonProperty("needs_talent_permissions")]
        public bool NeedsTalentPermissions { get; set; }
        [JsonProperty("needs_authentication")]
        public bool NeedsAuthentication { get; set; }

    }
}
