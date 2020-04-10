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
        private readonly ITagService _tagService;
        private readonly IResumeService _resumeService;

        public G2Controller(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _g2Service = services.GetService<IG2Service>(); 
            _hangfireService = services.GetService<IHangfireService>();
            _profileService = services.GetService<G2Interfaces.IProfileService>();
            _wishlistService = services.GetService<G2Interfaces.IWishlistService>();
            _commentService = services.GetService<G2Interfaces.ICommentService>();
            _skillService = services.GetService<ISkillService>();
            _tagService = services.GetService<ITagService>();
            _resumeService = services.GetService<IResumeService>();
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
        [Route("profiles/{profileGuid:guid}")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto profileDto, Guid profileGuid)
        {
            profileDto.ProfileGuid = profileGuid;
            await _profileService.UpdateProfileForRecruiter(profileDto, GetSubscriberGuid());
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize]
        [Route("profiles/{profileGuid:guid}/resume/is-exists")]
        public async Task<IActionResult> HasSubscriberUploadedResumeForRecruiter(Guid profileGuid)
        {
            var resume = await _resumeService.HasSubscriberUploadedResumeForRecruiter(profileGuid, GetSubscriberGuid());
            return Ok(resume);
        }


        [HttpGet]
        [Authorize]
        [Route("profiles/{profileGuid:guid}/resume")]
        public async Task<IActionResult> DownloadResumeForRecruiter(Guid profileGuid)
        {
            var resume = await _resumeService.DownloadResumeForRecruiter(profileGuid, GetSubscriberGuid());
            return Ok(resume);
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

        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("comments/profiles")]
        public async Task<IActionResult> CreateComments([FromBody] CommentsDto commentsDto)
        {
            var commentGuids = await _commentService.CreateCommentsForRecruiter(GetSubscriberGuid(), commentsDto);
            return StatusCode(201, commentGuids);
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

        [HttpPut]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("skills/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> UpdateSkillsForProfile([FromBody] List<Guid> skillGuids, Guid profileGuid)
        {
            await _skillService.UpdateProfileSkillsForRecruiter(GetSubscriberGuid(), skillGuids, profileGuid);
            return StatusCode(204);
        }

        #endregion

        #region Recruiter Tag Operations

        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("tags/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> AddTagsToProfile([FromBody] List<Guid> tagGuids, Guid profileGuid)
        {
            List<Guid> profileTagGuids = await _tagService.AddTagsToProfileForRecruiter(GetSubscriberGuid(), tagGuids, profileGuid);
            return StatusCode(201, profileTagGuids);
        }

        [HttpDelete]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("tags/profiles")]
        public async Task<IActionResult> DeleteTagsFromProfile([FromBody] List<Guid> profileTagGuids)
        {
            await _tagService.DeleteTagsFromProfileForRecruiter(GetSubscriberGuid(), profileTagGuids);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("tags/profiles/{profileGuid:guid}")]
        public async Task<IActionResult> GetTags(Guid profileGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var tags = await _tagService.GetProfileTagsForRecruiter(profileGuid, GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(tags);
        }

        #endregion
 
        #region G2 Query Functions 
        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("profiles/query")]
        public async Task<IActionResult> SearchG2(Guid cityGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0, bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null)
        {
            var rVal = await _g2Service.G2SearchAsync(GetSubscriberGuid(), cityGuid, limit, offset, sort, order, keyword, sourcePartnerGuid, radius, isWillingToRelocate,isWillingToTravel,isActiveJobSeeker,isCurrentlyEmployed,isWillingToWorkProBono );
            return Ok(rVal);
        }

        #endregion

        #region G2 Indexing Operations
 
        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure g2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> ReindexSubscriber(Guid subscriberGuid)
        {
            _g2Service.G2IndexBySubscriberAsync(subscriberGuid);
            return StatusCode(202);
        }

        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure g2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index/subscriber/{subscriberGuid}/company/{companyGuid}")]
        public async Task<IActionResult> ReindexSubscriberForCompany(Guid subscriberGuid, Guid companyGuid)
        {
            _g2Service.G2IndexBySubscriberAsync(subscriberGuid, companyGuid);
            return StatusCode(202);
        }

        #endregion

        #region ContactTypes

        [HttpGet("contactTypes")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetContactTypes()
        {
            var contactTypes = await _profileService.GetContactTypeList();
            return StatusCode((int)HttpStatusCode.OK, contactTypes);
        }

        [HttpGet("contactTypes/{id}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetContactTypeDetails(Guid id)
        {
            var contactType = await _profileService.GetContactTypeDetail(id);
            return StatusCode((int)HttpStatusCode.OK, contactType);
        }

        #endregion ContactTypes
               
        #region Admin Functions 

        // Admin functions will not be made public throught the APi gateway.  They are here for dev administration of the 
        // g2 profiles and azure index 

        /// <summary>
        /// Creates G2 profiles for all active subscriber/company combinations
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/index")]
        public async Task<IActionResult> addNewSubscribers()
        {
            // 
            _g2Service.G2AddNewSubscribers();
            return StatusCode(202);
        }


        /// <summary>
        /// Deletes all g2 records from the azure index 
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/index")]
        public async Task<IActionResult> deleteG2()
        {

            _g2Service.G2IndexPurgeAsync();
            return StatusCode(202);
        }



        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/companies/{companyGuid}")]
        public async Task<IActionResult> DeleteCompanyFromIndex(Guid companyGuid)
        {

            _g2Service.G2DeleteCompanyAsync(companyGuid);
            return StatusCode(202);
        }


        /// <summary>
        /// Add new company.  This will create a new G2 Profile for every active subscriber for the specified company  
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/companies/{companyGuid}")]
        public async Task<IActionResult> AddNewCompany(Guid companyGuid)
        {
            _g2Service.G2AddCompanyAsync(companyGuid);
            return StatusCode(202);
        }

        /// <summary>
        /// remove the subcribers profiles and remove them from the azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/profiles/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberFromIndex(Guid subscriberGuid)
        {

            _g2Service.G2DeleteSubscriberAsync(subscriberGuid);
            return StatusCode(202);
        }



        #endregion
    }
}