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
using UpDiddy.ViewModels;
using UpDiddyLib.Helpers;
using UpDiddyLib.Helpers.Braintree;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddyLib.Dto;

namespace WebApp.Controllers
{
    public class ServicesController : BaseController
    {
        private IApi _api;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly int _activeJobCount = 0;
        private IButterCMSService _butterService;
        private IBraintreeConfiguration braintreeConfiguration;

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
            braintreeConfiguration = new BraintreeConfiguration(_configuration);
        }

        [HttpGet("career-services")]
        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<ServicesPageViewModel> servicesPage = await _butterService.RetrievePageAsync<ServicesPageViewModel>("/career-services", QueryParams);

            ServicesPageViewModel servicesPageViewModel = new ServicesPageViewModel{
                HeroContent = servicesPage.Data.Fields.HeroContent,
                HeroTitle = servicesPage.Data.Fields.HeroTitle,
                PackagesFromCms = servicesPage.Data.Fields.PackagesFromCms
            };

            servicesPageViewModel.Packages = new List<PackageServiceViewModel>();

            foreach(Page<PackageServiceViewModel> page in servicesPageViewModel.PackagesFromCms){
                servicesPageViewModel.Packages.Add(new PackageServiceViewModel{
                    Title = page.Fields.Title,
                    MetaDescription = page.Fields.MetaDescription,
                    MetaKeywords = page.Fields.MetaKeywords,
                    OpenGraphTitle = page.Fields.OpenGraphTitle,
                    OpenGraphDescription = page.Fields.OpenGraphDescription,
                    OpenGraphImage = page.Fields.OpenGraphImage,
                    PackageName = page.Fields.PackageName,
                    TileImage = page.Fields.TileImage,
                    QuickDescription = page.Fields.QuickDescription,
                    Slug = page.Slug
                });
            }

            return View(servicesPageViewModel);
        }

        [HttpGet("career-services/{slug}")]
        public async Task<IActionResult> Details(string slug){
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            PageResponse<PackageServiceViewModel> packagePage = await _butterService.RetrievePageAsync<PackageServiceViewModel>("/career-services/" + slug, QueryParams);

            if(packagePage == null)
                return NotFound();

            ViewData[Constants.Seo.TITLE] = packagePage.Data.Fields.Title;
            ViewData[Constants.Seo.META_DESCRIPTION] = packagePage.Data.Fields.MetaDescription;
            ViewData[Constants.Seo.META_KEYWORDS] = packagePage.Data.Fields.MetaKeywords;
            ViewData[Constants.Seo.OG_TITLE] = packagePage.Data.Fields.OpenGraphTitle;
            ViewData[Constants.Seo.OG_DESCRIPTION] = packagePage.Data.Fields.OpenGraphDescription;
            ViewData[Constants.Seo.OG_IMAGE] = packagePage.Data.Fields.OpenGraphImage;

            PackageServiceViewModel packageServiceViewModel = new PackageServiceViewModel(){
                PackageName = packagePage.Data.Fields.PackageName,
                TileImage = packagePage.Data.Fields.TileImage,
                QuickDescription = packagePage.Data.Fields.QuickDescription,
                FullDescription = packagePage.Data.Fields.FullDescription,
                FullDescriptionHeader = packagePage.Data.Fields.FullDescriptionHeader
            };

            return View(packageServiceViewModel);
        }

        [HttpGet("career-services/{slug}/checkout")]
        public async Task<IActionResult> Checkout(string slug){
            PageResponse<PackageServiceViewModel> packagePage = await _butterService.RetrievePageAsync<PackageServiceViewModel>("/career-services/" + slug);

            if(packagePage == null)
                return NotFound();

            PackageServiceViewModel packageServiceViewModel = new PackageServiceViewModel(){
                PackageName = packagePage.Data.Fields.PackageName,
                TileImage = packagePage.Data.Fields.TileImage,
                QuickDescription = packagePage.Data.Fields.QuickDescription,
                FullDescription = packagePage.Data.Fields.FullDescription,
                FullDescriptionHeader = packagePage.Data.Fields.FullDescriptionHeader,
                Price = packagePage.Data.Fields.Price
            };

            var countries = await _Api.GetCountriesAsync();

            ServiceCheckoutViewModel serviceCheckoutViewModel = new ServiceCheckoutViewModel{
                PackageServiceViewModel = packageServiceViewModel,
                Countries = countries.Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString()
                }),
                States = new List<StateViewModel>().Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.StateGuid.ToString()
                }),
                Slug = slug
            };
            
            var gateway = braintreeConfiguration.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View(serviceCheckoutViewModel);
        }

        [HttpGet("career-services/{slug}/confirmation")]
        public async Task<IActionResult> Confirmation(string slug){
            PageResponse<PackageServiceViewModel> packagePage = await _butterService.RetrievePageAsync<PackageServiceViewModel>("/career-services/" + slug);

            if(packagePage == null)
                return NotFound();

            PackageServiceViewModel packageServiceViewModel = new PackageServiceViewModel(){
                PackageName = packagePage.Data.Fields.PackageName,
                TileImage = packagePage.Data.Fields.TileImage,
                QuickDescription = packagePage.Data.Fields.QuickDescription,
                FullDescription = packagePage.Data.Fields.FullDescription,
                FullDescriptionHeader = packagePage.Data.Fields.FullDescriptionHeader
            };

            PackageConfirmationViewModel packageConfirmationViewModel = new PackageConfirmationViewModel{
                PackageServiceViewModel = packageServiceViewModel
            };

            return View(packageConfirmationViewModel);
            
        }

        [HttpPost]
        public async Task<BasicResponseDto> SubmitPayment(ServiceCheckoutViewModel serviceCheckoutViewModel){

            if(!ModelState.IsValid)
                return new BasicResponseDto{ StatusCode = 400, Description = "Please correct your information and submit again."};
           

            return new BasicResponseDto{ StatusCode = 200, Description = "Success!"};
        }
    }
}