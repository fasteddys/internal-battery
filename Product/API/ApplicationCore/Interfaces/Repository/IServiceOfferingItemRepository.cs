
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IServiceOfferingItemRepository : IUpDiddyRepositoryBase<Models.ServiceOfferingItem>
    {
        Task<Models.ServiceOfferingItem> GetByNameAsync(string action);

    }
}
