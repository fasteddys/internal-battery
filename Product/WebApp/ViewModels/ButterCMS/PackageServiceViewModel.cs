using Newtonsoft.Json;

namespace UpDiddy.ViewModels.ButterCMS
{
    public class PackageServiceViewModel : ButterCMSBaseViewModel
    {
        [JsonProperty("package_name")]
        public string PackageName { get; set; }
        [JsonProperty("tile_image")]
        public string TileImage { get; set; }
        [JsonProperty("quick_description")]
        public string QuickDescription { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("full_description")]
        public string FullDescription { get; set; }
        [JsonProperty("full_description_header")]
        public string FullDescriptionHeader { get; set; }
        [JsonProperty("price")]
        public string Price { get; set; }
    }
}