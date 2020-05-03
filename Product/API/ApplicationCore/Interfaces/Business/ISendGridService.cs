using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISendGridService
    {
        Task<bool> SendBulkEmailByList(Guid TemplateGuid, List<Guid> profiles, Guid recruiterSubscriberGuid);

        Task<bool> SendUserDefinedBulkEmailByList(UserDefinedEmailDto userDefinedEmailDto, Guid recruiterSubscriberGuid, bool isTestEmail);

        Task<EmailTemplateListDto> GetEmailTemplates(int limit, int offset, string sort, string order);

    }
}
