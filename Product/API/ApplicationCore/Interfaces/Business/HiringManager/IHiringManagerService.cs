
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager
{
    public interface IHiringManagerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="nonBlocking"></param>
        /// <returns></returns>
        Task<bool> AddHiringManager(Guid subscriberGuid, bool nonBlocking = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<HiringManagerDto> GetHiringManagerBySubscriberGuid(Guid subscriberGuid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="hiringManager"></param>
        /// <returns></returns>
        Task UpdateHiringManager(Guid subscriberGuid, HiringManagerDto hiringManager);
    }
}




