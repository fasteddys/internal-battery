using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class HiringManagerDto
    {
        //Subscriber data
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Title { get; set; }

        public string City { get; set; }

        public Guid? StateGuid { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        //HM company fields
        public string CompanyName { get; set; }

        public int? CompanySize { get; set; }

        public string CompanyDescription { get; set; }

        public string CompanyWebsiteUrl { get; set; }

        public Guid? CompanyIndustryGuid { get; set; }

        //HM special Notes
        public string HardToFindFillSkillsRoles { get; set; }
        public string SkillsRolesWeAreAlwaysHiringFor { get; set; }

        [JsonIgnore]
        public Guid? HiringManagerGuid { get; set; }


    }
}
