using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    interface IProfileService
    {

        #region basic profile 
        /// <summary>
        /// Returns a subscribers basic profile   
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<SubscribeProfileBasicDto> GetSubscriberProfileBasicAsync(Guid subscriberGuid);

        /// <summary>
        /// Updates subscriber in the CareerCircle database using the subscriber guid provided and updates the subscriber in the 
        /// Google Talent Cloud.
        /// </summary>
        /// <param name="createUserDto"></param>
        /// <returns></returns>
        Task<bool> UpdateSubscriberProfileBasicAsync(SubscribeProfileBasicDto subscribeProfileBasicDto, Guid subscriberGuid);

        /// <summary>
        /// Creates subscriber in the CareerCircle database using the subscriber guid provided, adds the subscriber to the 
        /// Google Talent Cloud.
        /// </summary>
        /// <param name="createUserDto"></param>
        /// <returns></returns>
        Task<bool> CreateNewSubscriberAsync(SubscribeProfileBasicDto subscribeProfileBasicDto);

        #endregion

        #region social profile
        /// <summary>
        /// Updates subscriber's social profile in the CareerCircle database using the subscriber guid provided and updates the subscriber in the 
        /// Google Talent Cloud.
        /// </summary>
        /// <param name="createUserDto"></param>
        /// <returns></returns>
        Task<bool> UpdateSubscriberProfileSocialAsync(SubscriberProfileSocialDto subscribeProfileBasicDto, Guid subscriberGuid);



        /// <summary>
        /// Gets a subscriber's social profile in the CareerCircle database using the subscriber guid provided 
        /// </summary>
        /// <param name="createUserDto"></param>
        /// <returns></returns>
        Task<SubscriberProfileSocialDto> GetSubscriberProfileSocialAsync(Guid subscriberGuid);

        #endregion

        #region CareerPat

        /// <summary>
        /// Get a subscriber's careerpath (topic)
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<TopicDto> GetSubscriberCareerPath(Guid subscriberGuid);

        /// <summary>
        /// Updates the subscriber's selected career path (topic)
        /// </summary>
        /// <param name="careerPathGuid"></param>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task UpdateSubscriberCareerPath(Guid careerPathGuid, Guid subscriberGuid);
        
        #endregion

    }
}
