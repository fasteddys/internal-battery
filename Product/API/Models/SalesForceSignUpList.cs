using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class SalesForceSignUpList : BaseModel
    {
        public int SalesForceSignUpListId { get; set; }
        public Guid SalesForceSignUpListGuid {get;set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
