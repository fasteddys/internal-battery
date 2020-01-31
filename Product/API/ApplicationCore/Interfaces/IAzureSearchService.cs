using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyLib.Domain.AzureSearchDocuments;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IAzureSearchService
    {
        Task<bool> AddOrUpdate(Subscriber subscriber);
        Task<bool> Delete(Subscriber subscriber);
    }
}
