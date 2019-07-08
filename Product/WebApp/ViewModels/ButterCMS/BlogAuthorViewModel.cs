using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels.ButterCMS
{
    
    public class BlogAuthorViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("slug")]
        public string  Slug { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("linkedin_url")]
        public string LinkedinUrl { get; set; }

        [JsonProperty("facebook_url")]
        public string FacebookUrl { get; set; }

        [JsonProperty("pinterest_url")]
        public string PinterestUrl { get; set; }

        [JsonProperty("twitter_handle")]
        public string TwitterHandle { get; set; }

        [JsonProperty("profile_image")]
        public string ProfileImage { get; set; }
    } 
}
