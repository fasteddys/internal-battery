using System;

namespace UpDiddyLib.Domain.Models.Reports
{
    public class UsersDetailDto
    {
        public Guid SubscriberGuid { get; set; }
        public DateTime CreateDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string PartnerName { get; set; }
        public string GroupName { get; set; }
        public int EnrollmentsCreated { get; set; }
    }
}
