using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IActionRepository : IUpDiddyRepositoryBase<Models.Action>
    {
        Task<Models.Action> GetByNameAsync(string action);
        Task<Models.Action> GetActionByActionGuid(Guid actionGuid);
    }
}
