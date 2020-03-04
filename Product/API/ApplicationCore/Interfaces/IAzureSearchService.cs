using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IAzureSearchService
    {
        Task<bool> AddOrUpdateSubscriber(Subscriber subscriber);
        Task<bool> DeleteSubscriber(Subscriber subscriber);

        Task<bool> AddOrUpdateRecruiter(Recruiter recruiter);
        Task<bool> DeleteRecruiter(Recruiter recruiter);

        Task<bool> AddOrUpdateG2(G2SDOC g2);
        Task<bool> DeleteG2(G2SDOC g2);
        Task<bool> DeleteG2Bulk(List<G2SDOC> g2);
        Task<bool> AddOrUpdateG2Bulk(List<G2SDOC> g2);


    }
}
