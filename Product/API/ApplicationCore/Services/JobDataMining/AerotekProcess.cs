using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public class AerotekProcess : BaseProcess, IJobDataMining
    {
        public AerotekProcess(JobSite jobSite) : base(jobSite) { }

        public List<JobPage> GetJobPages()
        {
            throw new NotImplementedException();
        }

        public JobPosting ProcessJobPage(JobPage jobPage)
        {
            throw new NotImplementedException();
        }
    }
}
