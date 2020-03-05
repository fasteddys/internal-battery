using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISendGridEventService
    {
        Task<bool> AddSendGridEvent(SendGridEventDto sendGridEvent);

        Task<bool> AddSendGridEvents(List<SendGridEventDto> sendGridEvent);

        Task<bool> PurgeSendGridEvents(int lookbackDays);

    }
}
