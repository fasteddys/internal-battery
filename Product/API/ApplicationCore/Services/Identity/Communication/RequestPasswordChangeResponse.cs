using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.Identity.Communication
{
    public class RequestPasswordChangeResponse : BaseResponse
    {
        public string Ticket { get; private set; }

        public RequestPasswordChangeResponse(bool success, string message, string ticket): base(success, message)
        {
            Ticket = ticket;
        }
    }
}
