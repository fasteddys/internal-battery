using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public class AerotekProcess : BaseProcess, IJobDataMining
    {
        public AerotekProcess(JobSite jobSite, ILogger logger, Guid companyGuid) : base(jobSite, logger, companyGuid) { }

        public List<JobPage> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            throw new NotImplementedException();
        }

        public JobPostingDto ProcessJobPage(JobPage jobPage)
        {
            throw new NotImplementedException();
        }
    }
}
