using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace UpDiddy.ViewModels.ButterCMS
{ 
    public class BlogPostViewModel : ButterCMSBaseViewModel
    {
        /// <summary>
        /// The title of the post
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Unique identifier
        /// </summary>
        [JsonProperty("slug")]
        public string Slug { get; set; }

        /// <summary>
        /// The status of the post. Defaults to draft
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Author must exist in your account prior to POST. Should either be { "email" : "your@author.com" } or { “slug” : “firstname-lastname” }. Defaults to the organization owner.
        /// </summary>
        [JsonProperty("author")]
        public BlogAuthorViewModel Author { get; set; }        

        /// <summary>
        /// CDN URL of featured image
        /// </summary>
        [JsonProperty("featured_images")]
        public string FeaturedImages { get; set; }

        /// <summary>
        /// Full HTML Body. Should be an string of escaped HTML when creating a post.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// Plain-text summary
        /// </summary>
        [JsonProperty("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Use in HTML title tag
        /// </summary>
        [JsonProperty("seo_title")]
        public string SEOTitle { get; set; }

        [JsonProperty("categories")]
        public IEnumerable<CategoryViewModel> Categories { get; set; }

        [JsonProperty("tags")]
        public IEnumerable<TagViewModel> Tags { get; set; }


        [JsonProperty("created")]
        public string Created { get; set; }

        /// <summary>
        /// Timestamp of when the post was published
        /// </summary>
        [JsonProperty("published")]
        public string Published { get; set; }

    }    
}
