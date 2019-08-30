using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace UpDiddy.ViewModels
{
    public class TraitifyViewModel
    {
        public string AssessmentId { get; set; }
        public string PublicKey { get; set; }
        public string Host { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Invalid email address. Please update your supplied email and try again.")]
        public string Email { get; set; }

        [JsonProperty("hero_header")]
        public string HeroHeader { get; set; }
        [JsonProperty("hero_image")]
        public string HeroImage { get; set; }
        [JsonProperty("hero_description")]
        public string HeroDescription { get; set; }

        public bool? IsValid {get;set;}
        
    }
}
