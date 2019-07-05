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
    public class JobPostingService : IJobPostingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public JobPostingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        //Get similar job posting based on title and provice
        public async Task<List<JobPosting>> GetSimilarJobPostingsAsync(JobPosting jobPost)
        {
            IQueryable<JobPosting> allJobPostings = await _repositoryWrapper.JobPosting.GetAllAsync();
            return await allJobPostings
                .Where(x => x.Title.ToLower()
                .Contains(jobPost.Title) && x.Province == jobPost.Province && x.IsDeleted == 0)
                .OrderBy(y => y.PostingDateUTC)
                .ToListAsync();
        }
    }
}