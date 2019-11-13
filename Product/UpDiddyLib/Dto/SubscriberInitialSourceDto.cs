using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberInitialSourceDto
    {

        public int SubscriberId { get; set; }
        public DateTime CreateDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public bool IsVerified { get; set; }
        public string PartnerName { get; set; }
        public string GroupName { get; set; }
        public string LegacySource { get; set; }
        public int WozEnrollmentCount { get; set; }

    }
}
