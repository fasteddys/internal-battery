using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Services.JobDataMining;
using UpDiddyApi.ApplicationCore.Services.JobDataMining.ICIMS;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public static class JobDataMiningFactory
    {
        public static IJobDataMining GetJobDataMiningProcess(JobSite jobSite, IConfiguration config, ILogger logger, IEmploymentTypeService employmentTypeService)
        {
            // todo: remove hard-coded company guids (DI, repository, etc)
            switch (jobSite.Name)
            {
                case "Aerotek":                    
                    return new AerotekProcess(jobSite, logger, new Guid("7E1D8AB0-3440-4773-88B6-2722DA9F2FED"), config, employmentTypeService);
                case "TEKsystems":
                    return new TEKsystemsProcess(jobSite, logger, new Guid("2C2BC0D6-416B-4B62-A16A-87AC9B95D0B2"), config, employmentTypeService);
                case "Allegis Group ICIMS":
                    return new AllegisGroupProcess(jobSite, logger, new Guid("92728544-FDFD-493F-A7D3-5547DEA7B9DD"), config, employmentTypeService);
                case "EASi":
                    return new EASiProcess(jobSite, logger, new Guid("E7984C36-FD1C-4578-9E66-59305470EF16"), config, employmentTypeService);
                default:
                    throw new NotSupportedException($"Unrecognized and/or unsupported jobSite: {jobSite.Name}");
            }
        }
    }
}
