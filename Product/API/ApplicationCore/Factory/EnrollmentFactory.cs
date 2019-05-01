using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class EnrollmentFactory
    {
        public static Enrollment GetEnrollmentByGuid(UpDiddyDbContext db, Guid guid)
        {
            return db.Enrollment
                .Where(s => s.IsDeleted == 0 && s.EnrollmentGuid == guid)
                .FirstOrDefault();
        }


    }
}
