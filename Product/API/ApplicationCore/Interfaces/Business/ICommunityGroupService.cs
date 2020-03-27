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
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    /// <summary>
    /// Service that executes actions related to Subscribers.
    /// </summary>
    public interface ICommunityGroupService
    {
        /// <summary>
        /// Create a Community Group and return its guid
        /// </summary>
        /// <param name="subscriberDto"></param>
        /// <returns></returns>
        Task<Guid> CreateCommunityGroup(UpDiddyLib.Domain.Models.CommunityGroupDto communityGroupDto);


        /// <summary>
        ///  Updates existing community group info like name
        /// </summary>
        /// <param name="communityGroup"></param>
        /// <returns></returns>
        Task UpdateCommunityGroup(UpDiddyLib.Domain.Models.CommunityGroupDto communityGroupDto);

        /// <summary>
        /// Gets community group using the guid
        /// </summary>
        /// <param name="communityGroupGuid"></param>
        /// <returns></returns>
        Task<CommunityGroup> GetCommunityGroupByGuid(Guid communityGroupGuid);


        /// <summary>
        /// Gets community group using its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<CommunityGroup> GetCommunityGroupByName(string name);

   
        Task<bool> DeleteCommunityGroup(Guid communityGroupGuid);

        /// <summary>
        /// Imports a user resume
        /// </summary>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <param name="msg"></param>
        /// <returns></returns>

        Task<CommunityGroupSearchResultDto> SearchCommunityGroupsAsync(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*");


        /// <summary>
        /// Gets community group using the guid
        /// </summary>
        /// <param name="communityGroupSubscriberGuid"></param>
        /// <returns></returns>
        Task<CommunityGroupSubscriber> GetCommunityGroupSubscriber(Guid communityGroupSubscriberGuid);


        /// <summary>
        /// Gets community group using its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<Subscriber>> GetCommunityGroupSubscribers(Guid communityGroupGuid);

        /// <summary>
        /// Create a Community Group Subscriber and return its guid
        /// </summary>
        /// <param name="subscriberDto"></param>
        /// <returns></returns>
        Task<Guid> CreateCommunityGroupSubscriber(UpDiddyLib.Domain.Models.CommunityGroupSubscriberDto communityGroupSubscriberDto);

   
        Task<bool> DeleteCommunityGroupSubscriber(Guid communityGroupSubscriberGuid);

        /// <summary>
        /// Imports a user resume
        /// </summary>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <param name="msg"></param>
        /// <returns></returns>

        Task<CommunityGroupSubscriberSearchResultDto> SearchCommunityGroupSubscribersAsync(Guid communityGroupGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*");


    }
}