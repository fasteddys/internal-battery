using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;
using System.Security.Claims;
using UpDiddy.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using UpDiddy.Helpers;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.ViewModels;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class CourseController : BaseController
    {
        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;

        public CourseController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration) : base(azureAdB2COptions.Value, configuration)
        {
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            
            ApiUpdiddy API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, _configuration);
            //CourseViewModel CourseViewModel = new CourseViewModel(_configuration, API.Courses());
            //return View(CourseViewModel);
            return View();
        }
        
        [Authorize]
        public IActionResult Checkout()
        {
            setCurrentClientGuid();
            CheckoutViewModel checkoutViewModel = new CheckoutViewModel(this.subscriber);
            return View(checkoutViewModel);
        }





    }
}
