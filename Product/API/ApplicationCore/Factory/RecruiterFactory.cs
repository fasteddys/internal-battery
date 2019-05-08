using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class RecruiterFactory
    {
        public static Recruiter GetRecruiterBySubscriberId(UpDiddyDbContext db, int subscriberId)
        {
            return db.Recruiter
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .FirstOrDefault();
        }

        public static Recruiter GetRecruiterBySubscriberGuid(UpDiddyDbContext db, Guid subscriberGuid)
        {
            return db.Recruiter
                .Include ( s => s.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();
        }

        public static Recruiter GetRecruiterById(UpDiddyDbContext db, int recruiterId)
        {
            return db.Recruiter
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.RecruiterId == recruiterId)
                .FirstOrDefault();
        }

    }
}
