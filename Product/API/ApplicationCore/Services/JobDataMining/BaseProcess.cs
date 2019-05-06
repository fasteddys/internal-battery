using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public abstract class BaseProcess
    {
        protected JobSite _jobSite;

        public BaseProcess(JobSite jobSite)
        {
            _jobSite = jobSite;
        }
    }
}
