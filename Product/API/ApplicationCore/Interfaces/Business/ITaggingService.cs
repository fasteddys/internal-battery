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
        /// Adds a subscriber to any group associated with the refering url they signed
        /// up with. Allows for subscriber to be added to multiple groups if referer
        /// url is found multiple times, or multiple groups are associated with a
        /// single partner.
        /// </summary>
        /// <param name="SubscriberId"></param>
        /// <param name="RefererUrl"></param>
        Task<bool> AddSubscriberToGroupBasedOnReferrerUrlAsync(int SubscriberId, string RefererUrl);

        /// <summary>
        /// Adds subscriber to group associated with the partners that the subscriber's
        /// corresponding contact entry was associated with.
        /// </summary>
        /// <param name="SubscriberId"></param>
        Task<bool> AddConvertedContactToGroupBasedOnPartnerAsync(int SubscriberId);

        /// <summary>
        /// For express sign-ups where campaigns are created on the fly, ensure that
        /// a partnerreferrer entry exists so that the campaign can be mapped to a partner
        /// for subscriber tagging purposes.
        /// </summary>
        /// <param name="ReferrerUrl"></param>
        /// <param name="PartnerGuid"></param>
        Task<bool> EnsurePartnerReferrerEntryExistsIfPartnerSpecified(string ReferrerUrl, Guid PartnerGuid);
    }


}