using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace UpDiddy.ViewModels
{
    public class TraitifyViewModel
    {
        public string AssessmentId { get; set; }
        public string PublicKey { get; set; }
        public string Host { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Invalid email address. Please update your supplied email and try again.")]
        public string Email { get; set; }
        public string HeroHeader { get; set; }
        public string HeroImage { get; set; }
        public string HeroDescription { get; set; }
        public string ModalHeader {get;set;}
        public string ModalText {get;set;}
        public string FormHeader {get;set;}
        public string FormText {get;set;}
        public string FormButtonText {get;set;}
        public Guid? SubscriberGuid {get;set;}
        public bool IsAuthenticated {get;set;}
    }
}
