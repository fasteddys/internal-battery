using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadRequestDto
    {
        // todo: revisit data annotations in user story 369
        public bool? IsTest { get; set; }
        public string ExternalLeadIdentifier { get; set; }
        [Required]
        public string ApiToken { get; set; }
        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(100)]
        public string EmailAddress { get; set; }
        [Required]
        [RegularExpression("^([0-9]{0,3})?[2-9]{1}[0-9]{9}$", ErrorMessage = "Phone must be 10 digits and may not start with a 0 or 1.")]
        public string MobilePhone { get; set; }
        [MaxLength(100)]
        public string Address1 { get; set; }
        [MaxLength(100)]
        public string Address2 { get; set; }
        [MaxLength(100)]
        public string City { get; set; }
        [MaxLength(100)]
        public string State { get; set; }
        [MaxLength(12)]
        public string PostalCode { get; set; }
        [MaxLength(100)]
        public string Country { get; set; }
    }
}
