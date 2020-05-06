
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager
{
    public interface IHiringManagerService
    {
        Task<bool> AddHiringManager(Guid subscriberGuid, bool nonBlocking = true);

        Task UpdateHiringManager(Guid subscriberGuid, HiringManagerDto hiringManager);
    }
}




