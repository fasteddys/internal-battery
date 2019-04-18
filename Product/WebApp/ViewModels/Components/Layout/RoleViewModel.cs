using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.Components.Layout
{
    public class RoleViewModel
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("role_policy")]
        public string RolePolicy { get; set; }

    }
}
