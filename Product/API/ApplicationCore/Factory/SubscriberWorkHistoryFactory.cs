using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberWorkHistoryFactory
    {
        public static SubscriberWorkHistory GetWorkHistoryForSubscriber(UpDiddyDbContext db, Subscriber subscriber, Company company, DateTime startDate, DateTime endDate)
        {
            return db.SubscriberWorkHistory
                .Where(wh => wh.IsDeleted == 0 && wh.CompanyId == company.CompanyId && wh.SubscriberId == subscriber.SubscriberId && wh.StartDate == startDate && wh.EndDate == endDate)
                .FirstOrDefault();
        }

        public static bool AddWorkHistoryForSubscriber(UpDiddyDbContext db, Subscriber subscriber, SubscriberWorkHistoryDto workHistory, Company company)
        {

            bool rVal = true;
            try
            {
                SubscriberWorkHistory wh = new SubscriberWorkHistory()
                {
                    StartDate = workHistory.StartDate,
                    EndDate = workHistory.EndDate,
                    CompanyId = company.CompanyId,
                    SubscriberId = subscriber.SubscriberId,
                    Title = workHistory.Title,
                    JobDecription = workHistory.JobDecription,
                    IsCurrent = workHistory.IsCurrent,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyDate = DateTime.UtcNow,
                    ModifyGuid = Guid.Empty,
                    SubscriberWorkHistoryGuid = Guid.NewGuid(),
                    IsDeleted = 0
                };
                db.SubscriberWorkHistory.Add(wh);
                db.SaveChanges();
            }
            catch
            {
                rVal = false;
            }
            return rVal;

        }
    }
}
