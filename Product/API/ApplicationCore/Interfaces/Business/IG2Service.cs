using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IG2Service
    {
 
        #region G2 Searching
        Task<G2SearchResultDto> SearchG2Async(Guid subscriberGuid, int cityId,int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0);
        Task<G2SearchResultDto> GetTopG2sAsync(int numRecords);        
        #endregion

        #region G2 Azure Indexing 
        Task<bool> IndexG2Async(G2SDOC g2);
        Task<bool> IndexG2BulkAsync(List<G2SDOC> g2List);
        Task<bool> IndexSubscriberAsync(Guid subscriberGuid);
        Task<bool> G2IndexDeleteAsync(G2SDOC g2);
        Task<bool> G2IndexDeleteBulkAsync(List<G2SDOC> g2List);
        Task<bool> IndexSubscriberAsync(Guid subscriberGuid, Guid companyGuid);

        Task<bool> RemoveSubscriberFromIndexAsync(Guid subscriberGuid);
        Task<bool> IndexCompanyAsync(Guid companyuGuid);
        Task<bool> RemoveCompanyFromIndexAsync(Guid companyGuid);
        Task<bool> IndexBatchAsync(List<v_ProfileAzureSearch> g2Profiles);
        Task<bool> IndexProfileAsync(Guid g2ProfileGuid);



        #endregion

        #region G2 Backing Store Operations 
        Task<int> AddSubscriberProfilesAsync(Guid subscriberGuid);
        Task<int> DeleteSubscriberProfilesAsync(Guid subscriberGuid);
        Task<int> AddCompanyProfilesAsync(Guid companyGuid);
        Task<int> DeleteCompanyProfilesAsync(Guid companyGuid);
        #endregion

        #region   G2 Operations (backing store and indexing)
        Task<bool> AddSubscriberAsync(Guid subscriberGuid);
        Task<bool> DeleteSubscriberAsync(Guid subscriberGuid);
        Task<bool> AddCompanyAsync(Guid companyGuid);
        Task<bool> DeleteCompanyAsync(Guid companyGuid);
        #endregion


        #region admin functions

        Task<bool> CreateG2IndexAsync();
        Task<bool> PurgeG2IndexAsync();









        #endregion






    }
}
