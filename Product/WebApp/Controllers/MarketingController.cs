using System;
using System.Collections.Generic;
using System.Linq;
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
        
        [Authorize]
        [HttpGet]
        public PartialViewResult CampaignStatisticsGrid()
        {
            IList<CampaignStatisticDto> campaignsStatistics = _api.CampaignStatisticsSearch();
            return PartialView("_CampaignStatisticsGrid", campaignsStatistics);
        }
        
        [Authorize]
        [HttpGet]
        [Route("/Marketing/CampaignStatistics")]
        public IActionResult CampaignStatistics()
        {            
                IList<CampaignStatisticDto> campaignsStatistics = _api.CampaignStatisticsSearch();
                return View(campaignsStatistics);
        }


        [Authorize]
        [HttpGet]
        [Route("/Marketing/CampaignDetails/{CampaignGuid}")]
        public IActionResult CampaignDetails(Guid CampaignGuid)
        {
            CampaignDetailsViewModel viewModel = new CampaignDetailsViewModel();
            viewModel.CampaignGuid = CampaignGuid;
            viewModel.CampaignDetails = _api.CampaignDetailsSearch(CampaignGuid);
            return View(viewModel);
        }


        [Authorize]
        [HttpGet]
        public PartialViewResult CampaignDetailsGrid(CampaignDetailsViewModel campaignDetails)
        {         
           return PartialView("_CampaignDetailsGrid", campaignDetails);
        }


    }
}
