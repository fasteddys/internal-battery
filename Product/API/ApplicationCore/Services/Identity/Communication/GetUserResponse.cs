using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.Identity.Communication
{
    public class GetUserResponse : BaseResponse
    {
        public User User { get; private set; }

        public GetUserResponse(bool success, string message, User user) : base(success, message)
        {
            User = user;
        }
    }
}
