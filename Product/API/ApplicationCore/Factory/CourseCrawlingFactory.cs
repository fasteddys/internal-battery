using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.ITProTV;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CourseCrawlingFactory
    {
        public static ICourseProcess GetCourseProcess(CourseSite courseSite, IConfiguration config, ILogger logger, ISovrenAPI sovrenAPI)
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
