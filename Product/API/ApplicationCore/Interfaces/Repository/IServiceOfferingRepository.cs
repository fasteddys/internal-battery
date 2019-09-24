
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IServiceOfferingRepository : IUpDiddyRepositoryBase<Models.ServiceOffering>
    {
        Task<Models.ServiceOffering> GetByNameAsync(string action);
        Task<Models.ServiceOffering> GetByGuidAsync(Guid guid);
        Models.ServiceOffering GetByGuid(Guid guid);
        IList<ServiceOffering> GetAllServiceOfferings();


    }

}





 
 