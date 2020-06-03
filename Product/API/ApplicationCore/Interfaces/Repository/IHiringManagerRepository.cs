using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.Models.B2B;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IHiringManagerRepository : IUpDiddyRepositoryBase<HiringManager>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        Task AddHiringManager(int subscriberId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SubscriberId"></param>
        /// <returns></returns>
        Task<HiringManager> GetHiringManagerBySubscriberId(int SubscriberId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="hiringManagerDto"></param>
        /// <returns></returns>
        Task UpdateHiringManager(int subscriberId, HiringManagerDto hiringManagerDto);
    }
}
