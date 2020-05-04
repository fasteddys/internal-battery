
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;


namespace UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager
{
    public interface IHiringManagerService
    {
        Task<bool> AddHiringManager(Guid subscriberGuid);
    }
}




