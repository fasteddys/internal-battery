using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.CourseDataMining
{
    public abstract class BaseCourseProcess
    {
        protected CourseSite _courseSite;
        protected internal ILogger _syslog = null;
        protected IConfiguration _configuration;

        public BaseCourseProcess(CourseSite courseSite, ILogger logger, IConfiguration configuration)
        {
            _syslog = logger;
            _courseSite = courseSite;
            _configuration = configuration;
        }
    }
}
