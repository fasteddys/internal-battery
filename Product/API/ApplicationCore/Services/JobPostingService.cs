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
            var jobCount = await _repositoryWrapper.StoredProcedureRepository.GetJobCountPerProvince();
            var provinces = jobCount.Select(x => x.Province).Distinct();
            var jobCountDto = new List<JobPostingCountDto>();
            Enums.ProvinceName stateNameEnum;
            Enums.ProvincePrefix statePrefixEnum;
            foreach (var province in provinces)
            {
                var str = province.Trim().Replace(" ", "").ToUpper();
                string statePrefix = string.Empty;
                if (Enum.TryParse(str, out statePrefixEnum))
                {
                    statePrefix = str;
                }
                else if (Enum.TryParse(str, out stateNameEnum))
                {
                    Enums.ProvinceName value = (Enums.ProvinceName)(int)stateNameEnum;
                    statePrefix = value.ToString();
                }
                if (!String.IsNullOrEmpty(statePrefix))
                {
                    var distinctCompanies = jobCount
                    .Where(x => x.Province == province)
                    .Select(m => new { m.CompanyName, m.CompanyGuid, m.Count })
                    .OrderByDescending(x => x.Count);
                    List<JobPostingCompanyCountDto> companyCountdto = new List<JobPostingCompanyCountDto>();
                    foreach (var company in distinctCompanies)
                    {
                        companyCountdto.Add(new JobPostingCompanyCountDto()
                        {
                            CompanyGuid = company.CompanyGuid,
                            CompanyName = company.CompanyName,
                            JobCount = company.Count
                        });
                    }

                    if (companyCountdto.Count > 0)
                    {
                        var total = jobCount.Where(x => x.Province == province).Sum(s => s.Count);
                        jobCountDto.Add(new JobPostingCountDto(statePrefix, companyCountdto, total));
                    }
                }
            }
            return jobCountDto;
        }
    }
}