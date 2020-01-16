﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class OffersController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {

            _offerService = offerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOffers(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var offers = await _offerService.GetOffers(limit, offset, sort, order);
            return Ok(offers);
        }

        [HttpGet]
        [Route("{offer:guid}")]
        public async Task<IActionResult> GetOffer(Guid offer)
        {
            var offers = await _offerService.GetOffer(offer);
            return Ok(offers);
        }

        [HttpPost]
        [Route("{offer:guid}/claim")]
        [Authorize]
        public async Task<IActionResult> ClaimOffer(Guid offer)
        {
            await _offerService.ClaimOffer(GetSubscriberGuid(), offer);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("{offer:guid}/subscriber")]
        [Authorize]
        public async Task<IActionResult> HasSubscriberClaimedOffer(Guid offer)
        {
            var hasClaimedOffer = await _offerService.HasSubscriberClaimedOffer(GetSubscriberGuid(), offer);
            return Ok(hasClaimedOffer);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateOffer([FromBody] OfferDto offerDto)
        {
            await _offerService.CreateOffer(offerDto);
            return StatusCode(201);
        }

        [HttpPut]
        [Route("{offer:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateOffer(Guid offer, [FromBody]  OfferDto offerDto)
        {
            await _offerService.UpdateOffer(offer, offerDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{offer:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteOffer(Guid offer)
        {
            await _offerService.DeleteOffer(offer);
            return StatusCode(204);
        }
    }
}
