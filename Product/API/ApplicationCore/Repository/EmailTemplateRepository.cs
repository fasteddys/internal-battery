using Auth0.ManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using EmailTemplate = UpDiddyApi.Models.EmailTemplate;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EmailTemplateRepository : UpDiddyRepositoryBase<EmailTemplate>, IEmailTemplateRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public EmailTemplateRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<EmailTemplateDto>> GetEmailTemplates(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<EmailTemplateDto> templates = null;
            templates = await _dbContext.EmailTemplates.FromSql<EmailTemplateDto>("[DBO].[System_Get_EmailTemplates] @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return templates;
        }
    }
}
