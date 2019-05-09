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
        protected Guid _companyGuid = Guid.Empty;

        public BaseProcess(JobSite jobSite, ILogger logger, Guid companyGuid)
        {
            _syslog = logger;
            _jobSite = jobSite;
            _companyGuid = companyGuid;
        }
    }
}
