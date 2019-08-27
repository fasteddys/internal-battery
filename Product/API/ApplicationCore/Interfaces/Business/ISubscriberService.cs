using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    /// <summary>
    /// Service that executes actions related to Subscribers.
    /// </summary>
    public interface ISubscriberService
    {
        /// <summary>
        /// Updates the subscriber notification email setting.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="isNotificationEmailsEnabled"></param>
        /// <returns></returns>
        Task<bool> ToggleSubscriberNotificationEmail(Guid subscriberGuid, bool isNotificationEmailsEnabled);

        /// <summary>
        /// Adds Resume to subscriber.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="parseResume"></param>
        /// <returns></returns>
        Task<SubscriberFile> AddResumeAsync(Subscriber subscriber, IFormFile resumeDoc, bool parseResume);
        
        /// <summary>
        /// Creates subscriber using Partner Contact Guid. Potential user must be associated with Campaign, have associated Contact and PartnerContact.
        /// This will associate a resume with the subscriber upon creation if one is on file in PartnerContactFile.
        /// </summary>
        /// <param name="partnerContactGuid">PartnerContact Guid</param>
        /// <param name="signUpDto"></param>
        /// <returns>Subscriber</returns>
        Task<Subscriber> CreateSubscriberAsync(Guid partnerContactGuid, SignUpDto signUpDto);

        /// <summary>
        ///  Updates existing subscriber info like first name, last name and phone number
        /// </summary>
        /// <param name="signUpdto"></param>
        /// <returns></returns>
        Task UpdateSubscriber(Subscriber subscriber);

        /// <summary>
        /// Gets subscriber using the guid
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<Subscriber> GetSubscriberByGuid( Guid subscriberGuid); 

        /// <summary>
        /// Creates a background job to scan resume of subscriber if they have a resume on file.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns>bool</returns>
        Task<bool> QueueScanResumeJobAsync(Guid subscriberGuid);

        /// <summary>
        /// Returns a subscriber's resume as a Stream
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns>Stream containing a subscriber's resume</returns>
        Task<Stream> GetResumeAsync(Subscriber subscriber);

        /// <summary>
        /// Gets a mapping of JobPosting (Guid) to JobPostingFavorite (guid)
        /// </summary>
        /// <param name="subscriberGuid">subscriber guid in which to retrieve favorites for</param>
        /// <param name="jobGuids">List of Job Posting Guids</param>
        /// <returns>Dictionary<Guid, Guid></returns>
        Task<Dictionary<Guid, Guid>> GetSubscriberJobPostingFavoritesByJobGuid(Guid subscriberGuid, List<Guid> jobGuids);

        /// <summary>
        /// Add Subscriber Notes
        /// </summary>
        /// <param name="subscriberNotesDto"></param>
        /// <returns></returns>
        Task SaveSubscriberNotesAsync(SubscriberNotesDto subscriberNotesDto);

        /// <summary>
        /// Get Subscriber Notes List
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="searchquery"></param>
        /// <returns></returns>
        Task<List<SubscriberNotesDto>> GetSubscriberNotesBySubscriberGuid(string subscriberGuid, string recruiterGuid, string searchquery);

        Task<bool> DeleteSubscriberNote(Guid subscriberNotesGuid);

        /// <summary>
        /// Imports a user resume
        /// </summary>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task<bool> ImportResume(ResumeParse resumeParse, string resume);

        Task<Subscriber> GetSubscriber(ODataQueryOptions<Subscriber> options);

        Task<List<Subscriber>> GetSubscribersToIndexIntoGoogle(int numProfilesToProcess, int ndexVersion);

        Task<List<Subscriber>> GetFailedSubscribersSummaryAsync();


    }
}