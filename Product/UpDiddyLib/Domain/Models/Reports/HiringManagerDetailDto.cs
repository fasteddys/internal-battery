using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.Reports
{
   public class HiringManagerDetailDto
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
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public int? EmployeeSize { get; set; }
        public string WebsiteUrl { get; set; }
        public string HardToFindFillSkillsRoles { get; set; }
        public string SkillsRolesWeAreAlwaysHiringFor { get; set; }
        public string IndustryName { get; set; }
        public string PartnerName { get; set; }
        public string GroupName { get; set; }
    }
}
