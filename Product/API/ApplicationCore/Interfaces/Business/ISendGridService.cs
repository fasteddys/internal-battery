using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISendGridService
    {
        Task<bool> SendBulkEmailByList(Guid TemplateGuid, List<Guid> profiles, Guid subscriberId);
        Task<EmailTemplateListDto> GetEmailTemplates(int limit, int offset, string sort, string order);

    }
}
