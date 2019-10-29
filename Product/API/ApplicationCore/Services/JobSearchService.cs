using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobSearchService : IJobSearchService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public JobSearchService(IRepositoryWrapper repositoryWrapper) => _repositoryWrapper = repositoryWrapper;

        public async Task<int> GetActiveJobCount()
        {
           return await _repositoryWrapper.JobPosting.GetAll().Where(jp => jp.IsDeleted == 0).CountAsync();
        }

    }
}
