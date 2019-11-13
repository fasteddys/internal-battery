using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.Identity.Communication
{
    public class GetUsersResponse : BaseResponse
    {
        public List<User> Users { get; private set; }

        public GetUsersResponse(bool success, string message, List<User> users) : base(success, message)
        {
            Users = users;
        }
    }
}
