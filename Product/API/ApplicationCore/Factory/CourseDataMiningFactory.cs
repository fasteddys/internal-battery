using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services.CourseDataMining.Common;
using UpDiddyApi.ApplicationCore.Services.CourseDataMining.ITProTV;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CourseDataMiningFactory
    {
        public static ICourseProcess GetCourseDataMiningProcess(CourseSite courseSite, IConfiguration config, ILogger logger, ISovrenAPI sovrenAPI)
        {
            switch (courseSite.Name)
            {
                case "ITProTV":
                    return new ITProTVProcess(courseSite, logger, config, sovrenAPI);
                default:
                    throw new NotSupportedException($"Unrecognized and/or unsupported courseSite: {courseSite.Name}");                    
            }
        }
    }
}
