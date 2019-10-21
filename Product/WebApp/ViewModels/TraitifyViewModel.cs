using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace UpDiddy.ViewModels
{
    public class 
    TraitifyViewModel
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
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)|(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]))([A-Za-z\d@#$%^&*\-_+=[\]{}|\\:',?/`~""();!]|\.(?!@)){8,16}$", ErrorMessage = "Password must be 8-16 characters, containing 3 out of 4 of the following: Lowercase characters, uppercase characters, digits (0-9), and one or more of the following symbols: @ # $ % ^ & * - _ + = | ' , ? / ` ~ () ; .")]
        public string Password { get; set; }
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)|(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]))([A-Za-z\d@#$%^&*\-_+=[\]{}|\\:',?/`~""();!]|\.(?!@)){8,16}$", ErrorMessage = "Re-entered password must be 8-16 characters, containing 3 out of 4 of the following: Lowercase characters, uppercase characters, digits (0-9), and one or more of the following symbols: @ # $ % ^ & * - _ + = | ' , ? / ` ~ () ; .")]
        public string ReenterPassword { get; set; }
        public string HeroHeader { get; set; }
        public string HeroImage { get; set; }
        public string HeroDescription { get; set; }
        public string ModalHeader { get; set; }
        public string ModalText { get; set; }
        public string FormHeader { get; set; }
        public string FormText { get; set; }
        public string FormButtonText { get; set; }
        public string ExistingUserButtonText { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
