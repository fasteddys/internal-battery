using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services.JobDataMining;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public static class JobDataMiningFactory
    {
        public static IJobDataMining GetJobDataMiningProcess(JobSite jobSite, ILogger logger)
        {
            // todo: remove hard-coded company guids (DI, repository, etc)
            switch (jobSite.Name)
            {
                case "Aerotek":                    
                    return new AerotekProcess(jobSite, logger, new Guid("7E1D8AB0-3440-4773-88B6-2722DA9F2FED"));
                case "TEKsystems":
                    return new TEKsystemsProcess(jobSite, logger, new Guid("2C2BC0D6-416B-4B62-A16A-87AC9B95D0B2"));
                default:
                    throw new NotSupportedException($"Unrecognized and/or unsupported jobSite: {jobSite.Name}");
            }
        }
    }
}
