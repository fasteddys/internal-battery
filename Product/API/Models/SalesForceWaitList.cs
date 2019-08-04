using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class SalesForceWaitList : BaseModel
    {
        public int SalesForceWaitListId { get; set; }
        public Guid SalesForceWaitListGuid {get;set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
