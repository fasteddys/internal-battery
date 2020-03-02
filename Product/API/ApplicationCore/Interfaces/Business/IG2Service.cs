using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IG2Service
    {
        #region G2 Searching
        Task<G2SearchResultDto> SearchG2Async(Guid subscriberGuid, int cityId,int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0);
        #endregion

        #region G2 Azure Indexing 
        Task<bool> IndexG2Async(G2SDOC g2);
        Task<bool> IndexSubscriber(Guid subscriberGuid);
        Task<bool> RemoveSubscriberFromIndex(Guid subscriberGuid);
        #endregion

        #region G2 Backing Store Operations 
        Task<int> AddSubscriberProfiles(Guid subscriberGuid);
        Task<int> DeleteSubscriberProfiles(Guid subscriberGuid);
        #endregion

        #region   G2 Operations (backing store and indexing)
        Task<bool> AddSubscriber(Guid subscriberGuid);
        Task<bool> DeleteSubscriber(Guid subscriberGuid);
        Task<bool> AddCompany(Guid companyGuid);
        Task<bool> DeleteCompany(Guid companyGuid);

        #endregion






    }
}
