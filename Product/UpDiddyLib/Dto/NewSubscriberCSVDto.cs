using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class NewSubscriberCSVDto
    {

        int SubscriberId { get; set; }
        DateTime CreateDate { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        string City { get; set; }
        string State { get; set; }
        string PostalCode { get; set; }
        int IsVerfied { get; set; }
        string PartnerName { get; set; }
        string GroupName { get; set; }
        string LegacySource { get; set; }
        int WozEnrollmentCount { get; set; }

    }
}
