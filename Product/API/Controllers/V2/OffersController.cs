﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using UpDiddyApi.ApplicationCore.Interfaces.Business;
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
        public async Task<IActionResult> GetOffers(int limit = 5, int offset = 0)
        {
            var offers = await _offerService.GetAllOffers(limit,offset);
            return Ok(offers);
        }        
    }
}
