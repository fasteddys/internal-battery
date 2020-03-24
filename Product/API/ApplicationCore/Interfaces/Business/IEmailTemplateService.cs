using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplateListDto> GetEmailTemplates( int limit, int offset, string sort, string order);      
    }
}
