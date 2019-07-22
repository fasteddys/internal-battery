using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public JobPostingService(IRepositoryWrapper repositoryWrapper) => _repositoryWrapper = repositoryWrapper;
        /// <summary>
        /// Gets the job count based on state (province). It utilizes two types of enums (state prefix and state name)
        /// because the jobposting's province column has data that spells out state names and that uses abbreviations.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync()
        {
            var query = await _repositoryWrapper.JobPosting.GetAllAsync();
            var jobCount = new List<JobPostingCountDto>();
            Enums.ProvincePrefix statePrefixEnum;
            Enums.ProvinceName stateNameEnum;
            var provinceList = await query
                .Where(x => x.IsDeleted == 0)
                .Select(x => x.Province)
                .Distinct()
                .ToListAsync();
            foreach (var province in provinceList)
            {
                var str = province.Trim().Replace(" ", "");        
                int? stateId = null;
                if (Enum.TryParse(str.ToUpper(), out statePrefixEnum))
                {
                    stateId = (int)statePrefixEnum;
                }
                else if (Enum.TryParse(str.ToLower(), out stateNameEnum))
                {
                    stateId = (int)stateNameEnum;
                }
                if (stateId.HasValue)
                {
                    var companyQuery = await GetJobsByStateQuery(province);
                    if (companyQuery.Count > 0)
                    {
                        var total = companyQuery.Sum(c => c.JobCount);
                        jobCount.Add(new JobPostingCountDto(stateId.Value, companyQuery, total));
                    }
                }
            }
            return jobCount;
        }

        private async Task<List<JobPostingCompanyCountDto>> GetJobsByStateQuery(string province)
        {
            var query = await _repositoryWrapper.JobPosting.GetAllAsync();
            return query.Where(x => x.Province == province && x.IsDeleted == 0)
                .GroupBy(l => l.Company)
                .Select(g => new JobPostingCompanyCountDto()
                {
                    CompanyGuid = g.Key.CompanyGuid,
                    CompanyName = g.Key.CompanyName,
                    JobCount = g.Distinct().Count()
                })
                .OrderByDescending(x => x.JobCount)
                .Take(3)
                .ToList();
        }
    }
}