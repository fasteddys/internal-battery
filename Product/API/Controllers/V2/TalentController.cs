using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using System.Collections.Generic;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    [Route("/V2/[controller]/")]
    public class TalentController : BaseApiController
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ILogger _syslog = null;
        private readonly ITalentFavoriteService _talentFavoriteService = null;
        private readonly ITalentService _talentService = null;
        private IAuthorizationService _authorizationService;

        public TalentController(UpDiddyDbContext db
        , IMapper mapper
        , Microsoft.Extensions.Configuration.IConfiguration configuration
        , ILogger<ProfileController> sysLog
        , ISubscriberService subscriberService
        , IRepositoryWrapper repositoryWrapper  
        , ITalentFavoriteService talentFavoriteService
        , ITalentService talentService
        , IAuthorizationService authorizationService    )
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _talentFavoriteService = talentFavoriteService;
            _talentService = talentService;
            _authorizationService = authorizationService;
        }


        #region search lookups

        [HttpGet]
        [Route("order-by")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetOrderBy()
        {
            List<TalentSearchOrderByDto> rVal = await _talentService.GetTalentSearchOrderBy(); 
            return Ok(rVal);
        }




        [HttpGet]
        [Route("partners-filter")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetPartnersFilter()
        {
            List<TalentSearchPartnersFilterDto> rVal = await _talentService.GetTalentSearchPartnersFilter();
            return Ok(rVal);
        }

        #endregion


        #region talent query

        [HttpGet] 
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("search")]
        public async Task<IActionResult> TalentQuery(int limit, int offset, string orderBy, string keyword, string location, string partner)
        {

            var rVal = _talentService.SearchTalent(limit, offset, orderBy, keyword, location, partner);
            return Ok(rVal);
        }

        [HttpGet]
        [Authorize]
        [Route("search/{talent:guid}")]
        public async Task<IActionResult> TalentDetail(Guid talent)
        { 
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");
            var rVal = await _talentService.TalentDetails(GetSubscriberGuid(), talent, isAuth.Succeeded);
 
            return Ok(rVal);
        }

        #endregion



        #region talent favorites

        [HttpGet]
        [Route("favorites")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetFavoriteTalent(int limit, int offset, string sort, string order)
        {
            var isFavorite = await _talentFavoriteService.GetFavoriteTalent(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(isFavorite);
        }


        [HttpPost]
        [Route("{talent:guid}/favorites")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> AddTalentFavorite(Guid talent)
        {
            await _talentFavoriteService.AddToFavorite(GetSubscriberGuid(), talent);
            return StatusCode(201);
        }

        [HttpDelete]
        [Route("{talent:guid}/favorites")]
        [Authorize(Policy = "IsRecruiterPolicy")]

        public async Task<IActionResult> RemoveCourseFavorite(Guid talent)
        {
            await _talentFavoriteService.RemoveFromFavorite(GetSubscriberGuid(), talent);
            return StatusCode(204);
        }

        #endregion




    }
}