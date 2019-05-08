using Microsoft.Extensions.Logging;
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
        protected internal ILogger _syslog = null;

        public BaseProcess(JobSite jobSite, ILogger logger)
        {
            _syslog = logger;
            _jobSite = jobSite;
        }
    }
}
