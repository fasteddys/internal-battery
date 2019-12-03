using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
   public interface IPasswordResetRequestService
    {
        Task CreatePasswordResetRequest(Guid subscriberGuid);
        Task<bool> ConsumePasswordResetRequest(ResetPasswordDto resetPasswordDto);
        Task<bool> CheckValidityOfPasswordResetRequest(Guid passwordResetRequestGuid);
    }
}
