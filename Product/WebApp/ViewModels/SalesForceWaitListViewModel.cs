using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class SalesForceWaitListViewModel 
    {
        public Guid SalesForceWaitList { get; set; }

        [Required]
        [StringLength(10)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(10)]
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