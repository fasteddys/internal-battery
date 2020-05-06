using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IHiringManagerRepository : IUpDiddyRepositoryBase<HiringManager>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SubscriberId"></param>
        /// <returns></returns>
        Task<HiringManager> GetHiringManagerBySubscriberId(int SubscriberId);
    }
}
