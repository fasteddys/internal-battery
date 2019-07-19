using System.Collections.Generic;
using UpDiddyLib.Dto;
namespace UpDiddyLib.Dto
{
    public class JobPostingCountDto
    {
        public JobPostingCountDto(int stateId, List<JobPostingCompanyCountDto> CompanyPosting, int totalCount)
        {
            this.StateId = stateId;
            this.CompanyPosting = CompanyPosting;
            this.TotalCount = totalCount;
        }
        //This property is mapped to the enum in the library
        public int StateId { get; set; }
        public int TotalCount {get;set;}
        public List<JobPostingCompanyCountDto> CompanyPosting { get; set; }
    }
}