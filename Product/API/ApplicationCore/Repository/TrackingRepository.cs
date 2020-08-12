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
            SqlParameter slugParam = new SqlParameter();
            slugParam.ParameterName = "@sourceSlug";
            slugParam.SqlDbType = SqlDbType.VarChar;
            slugParam.Direction = ParameterDirection.Input;
            slugParam.Size = 150;
            slugParam.Value = sourceSlug;

            SqlParameter urlParam = new SqlParameter();
            urlParam.ParameterName = "@url";
            urlParam.SqlDbType = SqlDbType.VarChar;
            urlParam.Size = 2048;
            urlParam.Direction = ParameterDirection.Output;

            var sprocResponse = await _dbContext.Database.ExecuteSqlCommandAsync("[dbo].[System_Get_FullUrlFromSlugTrackingEventLog]  @sourceSlug = @sourceSlug, @url = @url OUTPUT", new object[] { slugParam, urlParam });
            string fullUrl = urlParam.Value.ToString();

            return fullUrl;
        }

        public async Task AddUpdateTracking(string url)
        {
            SqlParameter urlParam = new SqlParameter();
            urlParam.ParameterName = "@url";
            urlParam.SqlDbType = SqlDbType.VarChar;
            urlParam.Size = 2048;
            urlParam.Direction = ParameterDirection.Input;
            urlParam.Value = url;

            var sprocResponse = await _dbContext.Database.ExecuteSqlCommandAsync("[dbo].[System_Create_Update_LandingPageTrackingEvent] @url = @url", new object[] { urlParam } );
        }

    }
}
