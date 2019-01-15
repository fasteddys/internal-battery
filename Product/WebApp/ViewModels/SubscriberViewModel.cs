using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class SubscriberViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string LinkedInUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string StackOverflowUrl { get; set; }
        public string GithubUrl { get; set; }
        public List<SubscriberEducationHistoryDto> EducationHistory { get; set; }
        public List<EnrollmentDto> Enrollments { get; set; }
        public List<SkillDto> Skills { get; set; }
        public List<SubscriberWorkHistoryDto> WorkHistory { get; set; }
        public string CurrentTitle
        {
            get
            {
                return WorkHistory.Where(wh => wh.IsCurrent == 1).OrderByDescending(wh => wh.StartDate).FirstOrDefault()?.Title;
            }
        }
    }
}
