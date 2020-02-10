using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;



namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface ISubscriberEmailService
    {  
        Task<List<SubscriberEmailStatisticDto>> GetEmailStatistics(Guid subscriberGuid);
    }
}
