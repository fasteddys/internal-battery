using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.User
{
    public class JobDto
    {
        public Guid? JobPostingFavoriteGuid { get; set; }
        public JobPostingDto JobPosting { get; set; }
        public JobApplicationDto JobApplication { get; set; }
        public CompanyDto Company { get; set; }
    }
}