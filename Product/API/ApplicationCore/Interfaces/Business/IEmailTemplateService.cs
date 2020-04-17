using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplateListDto> GetEmailTemplates( int limit, int offset, string sort, string order);

        Task<IEnumerable<HydratedEmailBody>> HydrateEmailTemplate(string template, Guid recruiterGuid, IEnumerable<Guid> profileGuids);
    }
}
