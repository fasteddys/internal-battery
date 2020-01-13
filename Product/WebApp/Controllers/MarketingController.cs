using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    public class MarketingController : Controller
    {
        private IApi _api;
        public  MarketingController(IApi api)
        {
            _api = api;
        }
        
        /*
        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> CampaignStatisticsGrid()
        {
            IList<CampaignStatisticDto> campaignsStatistics = await _api.CampaignStatisticsSearchAsync();
            return PartialView("_CampaignStatisticsGrid", campaignsStatistics);
        }
        
        [Authorize]
        [HttpGet]
        [Route("/Marketing/CampaignStatistics")]
        public async Task<IActionResult> CampaignStatistics()
        {            
                IList<CampaignStatisticDto> campaignsStatistics = await _api.CampaignStatisticsSearchAsync();
                return View(campaignsStatistics);
        }


        [Authorize]
        [HttpGet]
        [Route("/Marketing/CampaignDetails/{CampaignGuid}/{CampaignName?}")]
        public async Task<IActionResult> CampaignDetails(Guid CampaignGuid, string CampaignName)
        {
            CampaignDetailsViewModel viewModel = new CampaignDetailsViewModel();
            viewModel.CampaignGuid = CampaignGuid;
            viewModel.CampaignName = CampaignName;
            viewModel.CampaignDetails = await _api.CampaignDetailsSearchAsync(CampaignGuid);
            return View(viewModel);
        }


        [Authorize]
        [HttpGet]
        public PartialViewResult CampaignDetailsGrid(CampaignDetailsViewModel campaignDetails)
        {         
           return PartialView("_CampaignDetailsGrid", campaignDetails);
        }
        */

    }
}
