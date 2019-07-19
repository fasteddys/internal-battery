using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using EntityTypeConst = UpDiddyLib.Helpers.Constants.EventType;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public JobApplicationService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<bool> IsSubscriberAppliedToJobPosting(int subscriberId, int jobPostingId)
        {
            IQueryable<JobApplication> jobPosting = await _repositoryWrapper.JobApplication.GetAllAsync();
            return await jobPosting.AnyAsync(x => x.SubscriberId == subscriberId && x.JobPostingId == jobPostingId);
        }     
    }
}