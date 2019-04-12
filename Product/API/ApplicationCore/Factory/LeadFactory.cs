using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class LeadFactory : FactoryBase
    {
        private const string CACHE_KEY = "GetAllLeadStatuses";
        private readonly UpDiddyDbContext _db;
        private readonly ILogger _syslog;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private List<LeadStatusDto> _allLeadStatuses = null;

        public LeadFactory(UpDiddyDbContext db, IConfiguration configuration, ILogger syslog, IDistributedCache distributedCache) : base(db, configuration, syslog, distributedCache)
        {
            _db = db;
            _distributedCache = distributedCache;
            _syslog = syslog;
            _configuration = configuration;
            _allLeadStatuses =
                new LeadStatusFactory(_db, _configuration, _syslog, _distributedCache)
                .GetAllLeadStatuses()
                .Select(ls => new LeadStatusDto()
                {
                    LeadStatusId = ls.LeadStatusId,
                    Message = ls.Description,
                    Severity = ls.Severity.ToString(),
                    Name = ls.Name
                })
                .ToList();
        }

        // todo: move all application logic from LeadController to LeadFactory!

        /// <summary>
        /// Performs a simplistic dupe check (name or email, infinite dupe window) on a lead. If a lead is identified as a duplicate, we update
        /// the lead statuses collection accordingly and set the lead as non-billable.
        /// </summary>
        /// <param name="leadRequest"></param>
        private void DupeCheck(LeadRequestDto leadRequest, ref List<LeadStatusDto> leadStatuses, ref bool isBillable)
        {
            var email = new SqlParameter("@Email", SqlDbType.NVarChar);
            email.Value = (object)leadRequest.EmailAddress ?? DBNull.Value;
            var phone = new SqlParameter("@Phone", SqlDbType.NVarChar);
            phone.Value = (object)leadRequest.MobilePhone ?? DBNull.Value;
            var isDupe = new SqlParameter { ParameterName = "@IsDupe", SqlDbType = SqlDbType.Bit, Size = 1, Direction = ParameterDirection.Output };
            var spParams = new object[] { email, phone, isDupe };
            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Get_LeadDupeCheck] 
                    @Email,
                    @Phone,
	                @IsDupe OUTPUT", spParams);
            if (Convert.ToBoolean(isDupe.Value))
            {
                leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Duplicate").FirstOrDefault());
                isBillable = false;
            }
        }

        public PartnerContact GetLead(Guid leadIdentifier)
        {
            return _db.PartnerContact
                .Where(pc => pc.IsDeleted == 0 && pc.PartnerContactGuid == leadIdentifier)
                .FirstOrDefault();
        }
    }
}
