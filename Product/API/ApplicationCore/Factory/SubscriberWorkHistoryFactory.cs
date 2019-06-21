using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberWorkHistoryFactory
    {

        public static SubscriberWorkHistory GetWorkHistoryByGuid(UpDiddyDbContext db, Guid SubcriberWorkHistoryGuid)
        {
            return db.SubscriberWorkHistory
                .Where(wh => wh.IsDeleted == 0 && wh.SubscriberWorkHistoryGuid == SubcriberWorkHistoryGuid )
                .FirstOrDefault();
        }

        public static SubscriberWorkHistory GetWorkHistoryForSubscriber(UpDiddyDbContext db, Subscriber subscriber, Company company, DateTime? startDate, DateTime? endDate)
        {
            return db.SubscriberWorkHistory
                .Where(wh => wh.IsDeleted == 0 && wh.CompanyId == company.CompanyId && wh.SubscriberId == subscriber.SubscriberId && wh.StartDate == startDate && wh.EndDate == endDate)
                .FirstOrDefault();
        }

        public static async Task<SubscriberWorkHistory> AddWorkHistoryForSubscriber(UpDiddyDbContext db, Subscriber subscriber, SubscriberWorkHistoryDto workHistory, Company company)
        {

            // html encode title and job description to be consistent with api endpoints that encode to protect again script injection 
            try
            {
                SubscriberWorkHistory wh = new SubscriberWorkHistory()
                {
                    StartDate = workHistory.StartDate,
                    EndDate = workHistory.EndDate,
                    CompanyId = company.CompanyId,
                    SubscriberId = subscriber.SubscriberId,
                    Title = HttpUtility.HtmlEncode(workHistory.Title),
                    JobDescription = HttpUtility.HtmlEncode(workHistory.JobDescription),
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
                return wh;
            }
            catch
            {
                return null;
            }
        

        }
    }
}
