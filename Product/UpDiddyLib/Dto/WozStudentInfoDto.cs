using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozStudentInfoDto
    {
        public int ExeterId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumberPrimary { get; set; }
        public string PhoneNumberSecondary { get; set; }
        public long LastLoginDateUTCTimeStamp { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
 
}
