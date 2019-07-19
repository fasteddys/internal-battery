using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITaggingService
    {
        /// <summary>
        /// Add a subscriber to a particular group. This method requires that a group
        /// be created in the Group table. This method assumes the subscriber exists
        /// for the SubscriberId being passed in.
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="SubscriberId"></param>
        Task<bool> AddSubscriberToGroupAsync(int GroupId, int SubscriberId);

        /// <summary>
        /// Adds subscriber to group associated with the partners that the subscriber's
        /// corresponding contact entry was associated with.
        /// </summary>
        /// <param name="SubscriberId"></param>
        Task<bool> AddConvertedContactToGroupBasedOnPartnerAsync(int SubscriberId);

        /// <summary>
        /// If a partner is specified, create a link in GroupPartner table, and 
        /// add a subscriber to a new group.
        /// </summary>
        /// <param name="ReferrerUrl"></param>
        /// <param name="PartnerGuid"></param>
        /// <param name="SubscriberId"></param>
        Task<bool> CreateGroup(string ReferrerUrl, Guid PartnerGuid, int SubscriberId);
    }


}