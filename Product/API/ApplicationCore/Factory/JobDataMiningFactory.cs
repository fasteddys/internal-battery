﻿using Microsoft.Extensions.Logging;
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
            switch (jobSite.Name)
            {
                case "Aerotek":
                    return new AerotekProcess(jobSite, logger);
                case "TEKsystems":
                    return new TEKsystemsProcess(jobSite, logger);
                default:
                    throw new NotSupportedException($"Unrecognized and/or unsupported jobSite: {jobSite.Name}");
            }
        }
    }
}
