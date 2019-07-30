using System;

namespace UpDiddyLib.Dto
{
    public class JobPostingCompanyCountDto
    {
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        public int JobCount { get; set; }
    }
}