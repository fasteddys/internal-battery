using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ContactDto : BaseDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid ContactGuid { get; set; }   
        public string FullName { get; set; }
    }
}
