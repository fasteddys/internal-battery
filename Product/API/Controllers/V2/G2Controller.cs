using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using G2Interfaces = UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto.User;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Domain.Models.G2;
using System.Collections.Generic;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class G2Controller : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly IG2Service _g2Service; 
        private readonly IHangfireService _hangfireService;
        private readonly G2Interfaces.IProfileService _profileService;
        private readonly G2Interfaces.IWishlistService _wishlistService;
        private readonly G2Interfaces.ICommentService _commentService;
        private readonly ISkillService _skillService;

        public G2Controller(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _g2Service = services.GetService<IG2Service>(); 
            _hangfireService = services.GetService<IHangfireService>();
            _profileService = services.GetService<G2Interfaces.IProfileService>();
            _wishlistService = services.GetService<G2Interfaces.IWishlistService>();
            _commentService = services.GetService<G2Interfaces.ICommentService>();
            _skillService = services.GetService<ISkillService>();
        }
 
        #region Recruiter Profile Operations
    
        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("profiles/{profileGuid:guid}")]
        public async Task<IActionResult> GetProfile(Guid profileGuid)
        {
            var profile = await _profileService.GetProfileForRecruiter(profileGuid, GetSubscriberGuid());
            return Ok(profile);
        }

        [HttpPut]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("profiles")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto profileDto)
        {                         
            await _profileService.UpdateProfileForRecruiter(profileDto, GetSubscriberGuid());
            return StatusCode(204);
        }

        #endregion

        #region Recruiter Wishlist Operations

        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists")]
        public async Task<IActionResult> CreateWishlist([FromBody] WishlistDto wishlistDto)
        {
            Guid wishlistGuid = await _wishlistService.CreateWishlistForRecruiter(GetSubscriberGuid(), wishlistDto);
            return StatusCode(201, wishlistGuid);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists/{wishlistGuid:guid}")]
        public async Task<IActionResult> GetWishlist(Guid wishlistGuid)
        {
            var wishlist = await _wishlistService.GetWishlistForRecruiter(wishlistGuid, GetSubscriberGuid());
            return Ok(wishlist);
        }

        [HttpPut]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists/{wishlistGuid:guid}")]
        public async Task<IActionResult> UpdateWishlist(Guid wishlistGuid, [FromBody] WishlistDto wishlistDto)
        {
            wishlistDto.WishlistGuid = wishlistGuid;
            await _wishlistService.UpdateWishlistForRecruiter(GetSubscriberGuid(), wishlistDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists/{wishlistGuid:guid}")]
        public async Task<IActionResult> DeleteWishlist(Guid wishlistGuid)
        {
            await _wishlistService.DeleteWishlistForRecruiter(GetSubscriberGuid(), wishlistGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists")]
        public async Task<IActionResult> GetWishlists(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var wishlists = await _wishlistService.GetWishlistsForRecruiter(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(wishlists);
        }

        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists/{wishlistGuid:guid}/profiles")]
        public async Task<IActionResult> AddProfilesToWishlist(Guid wishlistGuid, [FromBody] List<Guid> profileGuids)
        {
           List<Guid> profileWishlistGuids = await _wishlistService.AddProfileWishlistsForRecruiter(GetSubscriberGuid(), wishlistGuid, profileGuids);
            return StatusCode(201, profileWishlistGuids);
        }

        [HttpDelete]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists/profiles")]
        public async Task<IActionResult> DeleteProfilesFromWishlist([FromBody] List<Guid> profileWishlistGuids)
        {
            await _wishlistService.DeleteProfileWishlistsForRecruiter(GetSubscriberGuid(), profileWishlistGuids);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("wishlists/{wishlistGuid:guid}/profiles")]
        public async Task<IActionResult> GetWishlistProfiles(Guid wishlistGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var wishlistProfiles = await _wishlistService.GetProfileWishlistsForRecruiter(wishlistGuid, GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(wishlistProfiles);
        }

        #endregion

        #region Recruiter Comment Operations

        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("comments/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> CreateComment(Guid profileGuid, [FromBody] CommentDto commentDto)
        {
            commentDto.ProfileGuid = profileGuid;
            Guid commentGuid = await _commentService.CreateCommentForRecruiter(GetSubscriberGuid(), commentDto);
            return StatusCode(201, commentGuid);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("comments/{commentGuid:guid}")]
        public async Task<IActionResult> GetComment(Guid commentGuid)
        {
            var comment = await _commentService.GetCommentForRecruiter(commentGuid, GetSubscriberGuid());
            return Ok(comment);
        }

        [HttpPut]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("comments/{commentGuid:guid}")]
        public async Task<IActionResult> UpdateComment(Guid commentGuid, [FromBody] CommentDto commentDto)
        {
            commentDto.CommentGuid = commentGuid;
            await _commentService.UpdateCommentForRecruiter(GetSubscriberGuid(), commentDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("comments/{commentGuid:guid}")]
        public async Task<IActionResult> DeleteComment(Guid commentGuid)
        {
            await _commentService.DeleteCommentForRecruiter(GetSubscriberGuid(), commentGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("comments/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> GetComments(Guid profileGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var comments = await _commentService.GetProfileCommentsForRecruiter(profileGuid, GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(comments);
        }

        #endregion

        #region Recruiter Skill Operations

        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("skills/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> AddSkillsToProfile([FromBody] List<Guid> skillGuids, Guid profileGuid)
        {
            List<Guid> profileSkillGuids = await _skillService.AddSkillsToProfileForRecruiter(GetSubscriberGuid(), skillGuids, profileGuid);
            return StatusCode(201, profileSkillGuids);
        }
        
        [HttpDelete]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("skills/profiles")]
        public async Task<IActionResult> DeleteSkillsFromProfile([FromBody] List<Guid> profileSkillGuids)
        {
            await _skillService.DeleteSkillsFromProfileForRecruiter(GetSubscriberGuid(), profileSkillGuids);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("skills/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> GetSkills(Guid profileGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var skills = await _skillService.GetProfileSkillsForRecruiter(profileGuid, GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(skills);
        }

        #endregion

        #region Recruiter Tag Operations

        #endregion

        #region G2 Query Functions

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("query")]
        public async Task<IActionResult> SearchG2(int cityId, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0)
        {
            var rVal = await _g2Service.SearchG2Async(GetSubscriberGuid(), cityId, limit, offset, sort, order, keyword, radius);
            return Ok(rVal);
        }

        #endregion

        #region Admin Functions 

        /// <summary>
        /// Creates G2 profiles for all active subscriber/company combinations
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index")]
        public async Task<IActionResult> bootG2(Guid subscriberGuid)
        {
            // 
            _g2Service.CreateG2IndexAsync();
            return StatusCode(200);
        }

        /// <summary>
        /// Creates G2 profiles for all active subscriber/company combinations
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index")]
        public async Task<IActionResult> deleteG2(Guid subscriberGuid)
        {

            _g2Service.PurgeG2IndexAsync();
       
            return StatusCode(200);
        }


        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure g2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("subscriber/{subscriberGuid}")]
        public async Task<IActionResult> ReindexSubscriber(Guid subscriberGuid)
        {
            _g2Service.IndexSubscriberAsync(subscriberGuid);
            return StatusCode(200);
        }


        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure g2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("subscriber/{subscriberGuid}/company/{companyGuid}")]
        public async Task<IActionResult> ReindexSubscriberForCompany(Guid subscriberGuid, Guid companyGuid)
        {
            _g2Service.IndexSubscriberAsync(subscriberGuid, companyGuid);
            return StatusCode(200);
        }


        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("company/{companyGuid}")]
        public async Task<IActionResult> DeleteCompanyFromIndex(Guid companyGuid)
        {

            _g2Service.DeleteCompanyAsync(companyGuid);
            return StatusCode(204);
        }


        /// <summary>
        /// Add new company.  This will create a new G2 Profile for every active subscriber for the specified company  
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("company/{companyGuid}")]
        public async Task<IActionResult> AddNewCompany(Guid companyGuid)
        {
            _g2Service.AddCompanyAsync(companyGuid);
            return StatusCode(200);
        }

        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("subscriber/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberFromIndex(Guid subscriberGuid)
        {

            _g2Service.DeleteSubscriberAsync(subscriberGuid);
            return StatusCode(204);
        }


        /// <summary>
        /// Add new subscriber  
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("subscriber/{subscriberGuid}")]
        public async Task<IActionResult> AddNewSubscriber(Guid subscriberGuid)
        {
            _g2Service.AddSubscriberAsync(subscriberGuid);
            return StatusCode(200);
        }


        #endregion
    }
}