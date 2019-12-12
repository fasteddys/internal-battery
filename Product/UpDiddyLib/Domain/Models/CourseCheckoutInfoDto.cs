using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CourseCheckoutInfoDto
    {


        public string Name { get; set; }
        public string Description { get; set; }

        public string Code { get; set; }

        public string Slug { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public Guid CourseGuid { get; set; }

        public Guid SubscriberGuid { get; set; }

        public string TermsOfService { get; set; }

        public int TermsOfServiceId { get; set; }


        public List<SkillDto> Skills { get; set; }
 
        public List<CourseVariantCheckoutDto> CourseVariants { get; set; }
              
    }
}
