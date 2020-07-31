using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ITrackingRepository
    {
        Task AddUpdateTracking(string url);

        Task<string> GetFullUrlAfterTracking(string sourceSlug);
    }
}
