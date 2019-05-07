using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class RecruiterCompanyFactory
    {
        public static List<RecruiterCompany> GetRecruiterCompanyById(UpDiddyDbContext db, int subscriberId)
        {
            return db.RecruiterCompany
               .Include( s => s.Company)
               .Include( s => s.Recruiter.Subscriber)
               .Where(rc => rc.IsDeleted == 0 && rc.Recruiter.SubscriberId == subscriberId)               
               .ToList();
        }
    }
}
