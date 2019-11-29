using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
   public interface IPasswordResetRequestService
    {
        Task<Guid> CreatePasswordResetRequest(Guid subscriberGuid);
        Task<bool> ConsumePasswordResetRequest(Guid passwordResetRequestGuid);
    }
}
