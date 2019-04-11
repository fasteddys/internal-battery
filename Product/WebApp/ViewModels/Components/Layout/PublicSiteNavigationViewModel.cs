using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.Components.Layout
{
    public class PublicSiteNavigationViewModel<T>
    {
        [JsonProperty("careercircle_public_site_navigation")]
        public IList<T> CareerCirclePublicSiteNavigationRoot { get; set; }
    }
}
