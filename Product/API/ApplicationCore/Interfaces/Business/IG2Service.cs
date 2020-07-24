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
        // Prefix all search operations with G2Search...
        Task<G2SearchResultDto> G2SearchAsync(Guid subscriberGuid, Guid cityGuid,int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0,bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null);      
        Task<G2SearchResultDto> G2SearchGetTopAsync(int numRecords);
        #endregion



        #region G2 Azure Indexing 
        // Prefix all indexing operations with  G2Index....

        /// <summary>
        ///  index the provided G2 document into azure 
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        Task<bool> G2IndexAsync(G2SDOC g2);
        /// <summary>
        /// bulk index the list of g2 documents into azure 
        /// </summary>
        /// <param name="g2List"></param>
        /// <returns></returns>
        Task<bool> G2IndexBulkAsync(List<G2SDOC> g2List);

        Task<bool> G2IndexBulkDeleteByGuidAsync(List<Guid> guidList);

        /// <summary>
        /// index all g2s for given subscriber across all companies 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<bool> G2IndexBySubscriberAsync(Guid subscriberGuid);
        /// <summary>
        /// index g2 for given subscriber for given company 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        Task<bool> G2IndexBySubscriberAsync(Guid subscriberGuid, Guid companyGuid);

        /// <summary>
        /// remove the givent g2 document from azure 
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        Task<bool> G2IndexDeleteAsync(G2SDOC g2);

        /// <summary>
        /// remove the specified g2 documents from azure and mark the associated g2 profile record as index deleted 
        /// </summary>
        /// <param name="g2List"></param>
        /// <returns></returns>
        Task<bool> G2IndexDeleteBulkAsync(List<G2SDOC> g2List);

        /// <summary>
        /// remove the specified g2 documents from azure and mark the associated g2 profile record as index new so
        /// they can be redindex via a global reindex or indexer daemon
        /// </summary>
        /// <param name="g2List"></param>
        /// <returns></returns>
        Task<bool> G2IndexPurgeBulkAsync(List<G2SDOC> g2List);
     
        /// <summary>
        /// remove all instances of the subscriber for all companies from azure
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<bool> G2IndexRemoveSubscriberAsync(Guid subscriberGuid);

        /// <summary>
        /// add or update all profiles asscociated with the given company to azure 
        /// </summary>
        /// <param name="companyuGuid"></param>
        /// <returns></returns>
        Task<bool> G2IndexCompanyProflesAsync(Guid companyuGuid);

        /// <summary>
        /// remove all profiles asscociated with the given company to azure 
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        Task<bool> G2IndexRemoveCompanyProflesAsync(Guid companyGuid);

        /// <summary>
        /// index the given set of profiles 
        /// </summary>
        /// <param name="g2Profiles"></param>
        /// <returns></returns>
        Task<bool> G2IndexBulkByProfileAzureSearchAsync(List<v_ProfileAzureSearch> g2Profiles);


        /// <summary>
        /// index the profile specified by the supplied profile guid 
        /// </summary>
        /// <param name="g2ProfileGuid"></param>
        /// <returns></returns>
        Task<bool> G2IndexProfileByGuidAsync(Guid g2ProfileGuid);


        /// <summary>
        /// purge azure index of all documents 
        /// </summary>
        /// <returns></returns>
        Task<bool> G2IndexPurgeAsync();



        #endregion

        #region G2 Profile Operations
        // Prefix all G2 Profile backing store operations with G2Profile....

        /// <summary>
        /// Add profiles into the backing store for the specified subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<int> G2ProfileAddSubscriberAsync(Guid subscriberGuid);
        /// <summary>
        /// Delete profiles from the backing store for the specified subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<int> G2ProfileDeleteSubscriberAsync(Guid subscriberGuid);
        /// <summary>
        /// Add profiles for every active subscriber for the specified company 
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        Task<int> G2ProfileAddByCompany(Guid companyGuid);
        /// <summary>
        /// Delete profiles for for every subscriber associated with the specified company
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        Task<int> G2ProfileDeleteByCompany(Guid companyGuid);
        #endregion

        #region   G2 Operations
        // prefix all G2 operations with G2.....
        Task<bool> G2AddSubscriberAsync(Guid subscriberGuid);
        Task<bool> G2DeleteSubscriberAsync(Guid subscriberGuid);
        Task<bool> G2AddCompanyAsync(Guid companyGuid);
        Task<bool> G2DeleteCompanyAsync(Guid companyGuid);
        Task<bool> G2AddNewSubscribers();
        #endregion



        // TODO JAB Move to Candidate Service 
        #region B2B Searching
        Task<G2SearchResultDto> HiringManagerSearchAsync(Guid subscriberGuid, Guid cityGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0, bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null);
        #endregion








    }
}
