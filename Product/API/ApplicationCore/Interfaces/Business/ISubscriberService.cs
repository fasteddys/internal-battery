using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    /// <summary>
    /// Service that executes actions related to Subscribers.
    /// </summary>
    public interface ISubscriberService
    {
        /// <summary>
        /// Adds Resume to subscriber.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="parseResume"></param>
        /// <returns></returns>
        Task<SubscriberFile> AddResumeAsync(Subscriber subscriber, string fileName, Stream fileStream, bool parseResume);
        
        /// <summary>
        /// Creates subscriber using Partner Contact Guid. Potential user must be associated with Campaign, have associated Contact and PartnerContact.
        /// This will associate a resume with the subscriber upon creation if one is on file in PartnerContactFile.
        /// </summary>
        /// <param name="partnerContactGuid">PartnerContact Guid</param>
        /// <param name="signUpDto"></param>
        /// <returns>Subscriber</returns>
        Task<Subscriber> CreateSubscriberAsync(Guid partnerContactGuid, SignUpDto signUpDto);

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
    }
}
