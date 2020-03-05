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
        private readonly ITalentNoteService _talentNoteService = null;
        private readonly IResumeService _resumeService;

        public TalentController(UpDiddyDbContext db
        , IMapper mapper
        , Microsoft.Extensions.Configuration.IConfiguration configuration
        , ILogger<ProfileController> sysLog
        , ISubscriberService subscriberService
        , IRepositoryWrapper repositoryWrapper
        , ITalentFavoriteService talentFavoriteService
        , ITalentService talentService
        , IAuthorizationService authorizationService
        , ITalentNoteService talentNoteService
        , IResumeService resumeService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _talentFavoriteService = talentFavoriteService;
            _talentService = talentService;
            _authorizationService = authorizationService;
            _talentNoteService = talentNoteService;
            _resumeService = resumeService;
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

        [HttpGet]
        [Authorize]
        [Route("search/{talent:guid}/resume")]
        public async Task<IActionResult> TalentResume(Guid talent)
        {
            var resume = await _resumeService.DownloadResume(talent);
            return Ok(resume);
        }



        #endregion

        #region talent favorites

        [HttpGet]
        [Route("favorites")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetFavoriteTalent(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var isFavorite = await _talentFavoriteService.GetFavoriteTalent(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(isFavorite);
        }

        [HttpPost]
        [Route("{talent:guid}/favorites")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> AddTalentFavorite(Guid talent)
        {
            var talentFavoriteGuid = await _talentFavoriteService.AddToFavorite(GetSubscriberGuid(), talent);
            return StatusCode(201, talentFavoriteGuid);
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

        #region talent notes


        [HttpPost]
        [Route("{talent:guid}/notes")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> AddTalentNote([FromBody] SubscriberNotesDto subscriberNotesDto, Guid talent)
        {
            var subscriberNoteGuid = await _talentNoteService.CreateNote(GetSubscriberGuid(), talent, subscriberNotesDto);
            return StatusCode(201, subscriberNoteGuid);
        }

        [HttpPut]
        [Route("{talent:guid}/notes/{note:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> AddTalentNote([FromBody] SubscriberNotesDto subscriberNotesDto, Guid talent, Guid note)
        {
            await _talentNoteService.UpdateNote(GetSubscriberGuid(), talent, note, subscriberNotesDto);
            return StatusCode(200);
        }


        [HttpDelete]
        [Route("{talent:guid}/notes/{note:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> DeleteTalentNote(Guid talent, Guid note)
        {
            await _talentNoteService.DeleteNote(GetSubscriberGuid(), note);
            return StatusCode(204);
        }

        [HttpGet]
        [Route("{talent:guid}/notes/{note:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetTalentNote(Guid talent, Guid note)
        {
            SubscriberNotesDto rVal = await _talentNoteService.GetNote(GetSubscriberGuid(), note);
            return Ok(rVal);
        }



        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("{talent:guid}/notes")]
        public async Task<IActionResult> TalentNoteList(Guid talent, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {

            var rVal = await _talentNoteService.GetNotesForSubscriber(GetSubscriberGuid(), talent, limit, offset, sort, order);
            return Ok(rVal);
        }

        #endregion




    }
}