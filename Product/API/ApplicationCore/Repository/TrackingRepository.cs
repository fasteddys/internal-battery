using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TrackingRepository : UpDiddyRepositoryBase<Tracking>, ITrackingRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public TrackingRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GetFullUrlAfterTracking(string sourceSlug)
        {
            var spParams = new object[] {
                new SqlParameter("@sourceSlug", sourceSlug)
                };
            string fullUrl = _dbContext.TrackingPageSlugs.FromSql<string>("[dbo].[System_Get_FullUrlFromSlugTrackingEventLog]  @sourceSlug", spParams).ToString();

            //await _dbContext.Query<string>("[G2].[System_Get_ProfileSkillsForRecruiter] @ProfileGuid, @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return fullUrl;

            //var tracking = await _dbContext.Tracking.FirstOrDefaultAsync(t => t.SourceSlug.Equals(url.Trim(), StringComparison.OrdinalIgnoreCase));

            //if (tracking == null) return null;

            //var trackingEventDay = await _dbContext.TrackingEventDay.
            //    FirstOrDefaultAsync(t => t.TrackingId == tracking.TrackingId &&
            //                             t.Day.Date.Equals(DateTime.UtcNow.Date));

            //if(trackingEventDay != null)
            //{
            //    //update count on existing tracking record
            //    trackingEventDay.Count = trackingEventDay.Count + 1;
            //    trackingEventDay.ModifyDate = DateTime.UtcNow;
            //}
            //else
            //{
            //    //add new tracking record
            //    _dbContext.TrackingEventDay.Add(new TrackingEventDay
            //    {
            //        CreateDate = DateTime.UtcNow,
            //        CreateGuid = Guid.Empty,
            //        Tracking = tracking,
            //        TrackingEventDayGuid = Guid.NewGuid(),
            //        Count = 1,
            //        Day = DateTime.UtcNow.Date,
            //        IsDeleted = 0
            //    });
            //}

            //await _dbContext.SaveChangesAsync();

            //return tracking;

        }

        public async Task AddUpdateTracking(string url)
        {
            var spParams = new object[] {
                new SqlParameter("@url", url)
                };

            var fullUrl = _dbContext.Database.ExecuteSqlCommand("[dbo].[System_Create_Update_LandingPageTrackingEvent] @url", spParams);
        }

    }
}
