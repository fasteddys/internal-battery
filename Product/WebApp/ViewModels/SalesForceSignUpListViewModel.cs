using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class SalesForceSignUpListViewModel 
    {
        public Guid SalesForceSignUpList { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
    }
}