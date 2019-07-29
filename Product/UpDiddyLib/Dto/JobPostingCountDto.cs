using System.Collections.Generic;
using UpDiddyLib.Dto;
namespace UpDiddyLib.Dto
{
    public class JobPostingCountDto
    {
        public JobPostingCountDto(string statePrefix, List<JobPostingCompanyCountDto> CompanyPosting, int totalCount)
        {
            this.StatePrefix = statePrefix;
            this.CompanyPosting = CompanyPosting;
            this.TotalCount = totalCount;
        }
        //This property is mapped to the enum in the library
        public string StatePrefix { get; set; }
        public int TotalCount {get;set;}
        public List<JobPostingCompanyCountDto> CompanyPosting { get; set; }
    }
}