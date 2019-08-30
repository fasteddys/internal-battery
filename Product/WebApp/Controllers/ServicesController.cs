using System.Collections.Generic;
using System.Threading.Tasks;
using ButterCMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddy.Controllers;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;


namespace WebApp.Controllers
{
    public class ServicesController : BaseController
    {
        private IApi _api;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly int _activeJobCount = 0;
        private IButterCMSService _butterService;

        public ServicesController(IApi api,
        IConfiguration configuration,
        IHostingEnvironment env,
        IButterCMSService butterService)
         : base(api)
        {
            _api = api;
            _env = env;
            _configuration = configuration;
            _butterService = butterService;
        }

        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<ServicesPageViewModel> servicesPage = await _butterService.RetrievePageAsync<ServicesPageViewModel>("/services", QueryParams);

            ServicesPageViewModel servicesPageViewModel = new ServicesPageViewModel{
                HeroContent = servicesPage.Data.Fields.HeroContent,
                HeroTitle = servicesPage.Data.Fields.HeroTitle
            };

            return View(servicesPageViewModel);
        }
    }
}