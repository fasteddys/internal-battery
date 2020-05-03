using Auth0.ManagementApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using EmailTemplate = UpDiddyApi.Models.EmailTemplate;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IEmailTemplateRepository : IUpDiddyRepositoryBase<EmailTemplate>
    {
        Task<List<EmailTemplateDto>> GetEmailTemplates(int limit, int offset, string sort, string order);
    }
}
