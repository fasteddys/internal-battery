using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services.CourseDataMining;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CourseDataMiningFactory
    {
        public static ICourseDataMining GetCourseDataMiningProcess(CourseSite courseSite, IConfiguration config, ILogger logger)
        {
            switch (courseSite.Name)
            {
                case "ITProTV":
                    return new ITProTVProcess(courseSite, logger, config);
                default:
                    throw new NotSupportedException($"Unrecognized and/or unsupported courseSite: {courseSite.Name}");                    
            }
        }
    }
}
