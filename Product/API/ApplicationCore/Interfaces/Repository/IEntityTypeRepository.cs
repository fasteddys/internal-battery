using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IEntityTypeRepository : IUpDiddyRepositoryBase<EntityType>
    {
        Task<EntityType> GetByNameAsync(string entityTypeName);
        Task<EntityType> GetEntityTypeByEntityGuid(Guid? entityTypeGuid);
    }
}
