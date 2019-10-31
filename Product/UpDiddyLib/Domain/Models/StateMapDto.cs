using System.Collections.Generic;
namespace UpDiddyLib.Domain.Models
{
    public class StateMapDto
    {
        public StateMapDto(string statePrefix, List<StateMapCompanyDto> CompanyPosting, int totalCount)
        {
            this.StatePrefix = statePrefix;
            this.CompanyPosting = CompanyPosting;
            this.totalJobCount = totalCount;
        }
        public string StatePrefix { get; set; }
        public int totalJobCount { get; set; }
        public List<StateMapCompanyDto> CompanyPosting { get; set; }
    }
}