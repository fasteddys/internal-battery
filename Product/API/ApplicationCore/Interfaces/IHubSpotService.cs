using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IHubSpotService
    {
        Task<long> AddOrUpdateContactBySubscriberGuid(Guid subscriberGuid, DateTime? lastLoginDateTime = null, bool nonBlocking = true);
    }
}
