using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using System;
using UpDiddyLib.Helpers;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public JobPostingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }
        public async Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync()
        {
            var query = await _repositoryWrapper.JobPosting.GetAllAsync();
            List<JobPostingCountDto> jobCount = new List<JobPostingCountDto>();
            List<string> provinceList = await query
            .Where(x => x.IsDeleted == 0)
            .Select(x => x.Province)
            .Distinct()
            .ToListAsync();
            foreach (var province in provinceList)
            {
                var str = province.Trim().Replace(" ", "");
                Enums.ProvincePrefix stateEnum;
                if (Enum.TryParse(str.ToUpper(), out stateEnum))
                {
                    var companyQuery = query.Where(x => x.Province == province && x.IsDeleted == 0)
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
                    if (companyQuery.Count > 0)
                    {
                        var total = companyQuery.Sum(c => c.JobCount);
                        jobCount.Add(new JobPostingCountDto((int)stateEnum, companyQuery, total));
                    }
                }
            }
            return jobCount;
        }
    }
}