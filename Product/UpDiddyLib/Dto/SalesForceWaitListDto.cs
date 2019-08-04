using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SalesForceWaitListDto : BaseDto
    {
        public Guid SalesForceWaitListGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
    }
}
