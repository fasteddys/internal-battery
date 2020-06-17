using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICommuteDistancesRepository
    {
        Task<List<CommuteDistance>> GetCommuteDistances(int limit, int offset, string sort, string order);
        Task<CommuteDistance> GetCommuteDistanceByGuid(Guid coummuteDistanceGuid);
    }
}
