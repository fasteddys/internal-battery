using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadRequestDto
    {     
        public bool? IsTest { get; set; }
        public string ExternalLeadIdentifier { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(30, ErrorMessage = "First name must not exceed 30 characters")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(30, ErrorMessage = "Last name must not exceed 30 characters")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "EmailAddress is required")]
        [MaxLength(100, ErrorMessage = "EmailAddress must not exceed 100 characters")]
        public string EmailAddress { get; set; }
        [RegularExpression("^([0-9]{0,3})?[2-9]{1}[0-9]{9}$", ErrorMessage = "Phone must be 10 digits and may not start with a 0 or 1")]
        public string MobilePhone { get; set; }
        [MaxLength(100, ErrorMessage = "Address1 must not exceed 100 characters")]
        public string Address1 { get; set; }
        [MaxLength(100, ErrorMessage = "Address2 must not exceed 100 characters")]
        public string Address2 { get; set; }
        [MaxLength(100, ErrorMessage = "City must not exceed 100 characters")]
        public string City { get; set; }
        [MaxLength(100, ErrorMessage = "State must not exceed 100 characters")]
        public string State { get; set; }
        [MaxLength(12, ErrorMessage = "PostalCode must not exceed 100 characters")]
        public string PostalCode { get; set; }
        [MaxLength(100, ErrorMessage = "Country must not exceed 100 characters")]
        public string Country { get; set; }
    }
}
