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
using UpDiddyLib.Dto.Marketing;
using System.Security.Claims;
using System;

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

            if(servicesPage == null)
                return NotFound();

            ServicesPageViewModel servicesPageViewModel = new ServicesPageViewModel{
                HeroContent = servicesPage.Data.Fields.HeroContent,
                HeroTitle = servicesPage.Data.Fields.HeroTitle,
                HeroImage = servicesPage.Data.Fields.HeroImage,
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
                    Slug = page.Slug,
                    Price = page.Fields.Price
                });
            }

            return View(servicesPageViewModel);
        }

        [HttpGet("career-services/{slug:length(0,100)}")]
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
                FullDescriptionHeader = packagePage.Data.Fields.FullDescriptionHeader,
                Price = packagePage.Data.Fields.Price
            };

            return View(packageServiceViewModel);
        }

        [HttpGet("career-services/{slug:length(0,100)}/checkout")]
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
                Price = packagePage.Data.Fields.Price,
                PackageId = packagePage.Data.Fields.PackageId
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

        [HttpGet("career-services/{slug:length(0,100)}/confirmation/{orderGuid}")]
        public async Task<IActionResult> Confirmation(string slug, Guid orderGuid){
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

            ServiceOfferingOrderDto serviceOfferingOrderDto = null;
            try{
                serviceOfferingOrderDto = await _api.GetSubscriberOrder(orderGuid);
            }
            catch(ApiException e){
                return NotFound();
            }
                

            PackageConfirmationViewModel packageConfirmationViewModel = new PackageConfirmationViewModel{
                PackageServiceViewModel = packageServiceViewModel,
                ServiceOfferingOrderDto = serviceOfferingOrderDto
            };

            return View(packageConfirmationViewModel);
            
        }

        [HttpPost("/services/promo-code/validate")]
        public async Task<PromoCodeDto> ValidatePromoCodeAsync(ServiceCheckoutViewModel serviceCheckoutViewModel){
            PromoCodeDto promoCodeDto = await _api.ServiceOfferingPromoCodeValidationAsync(serviceCheckoutViewModel.PromoCodeEntered, serviceCheckoutViewModel.PackageServiceViewModel.PackageId);
            return promoCodeDto;
        }

        [HttpPost]
        public async Task<BasicResponseDto> SubmitPayment(ServiceCheckoutViewModel serviceCheckoutViewModel){
            
            // This should only be triggered if someone alters the client side js validation
            if(!ModelState.IsValid)
                return new BasicResponseDto{ StatusCode = 400, Description = "Please correct your information and submit again."};

            bool IsNewSubscriberCheckout = IsCheckoutSignUp(serviceCheckoutViewModel);

            // Validate passwords match if a new subscriber checkout
            if(IsNewSubscriberCheckout && !PasswordsMatch(serviceCheckoutViewModel)){
                return new BasicResponseDto{ StatusCode = 400, Description = "The passwords you've entered do not match. Please correct them and try again."};
            }

            string Slug = serviceCheckoutViewModel.Slug;

            if (Slug.Length > 100)
                return new BasicResponseDto { StatusCode = 400, Description = "Unexpected page path specified." };

            PageResponse<PackageServiceViewModel> packagePage = await _butterService.RetrievePageAsync<PackageServiceViewModel>("/career-services/" + Slug);

            if(packagePage == null)
                return new BasicResponseDto{ StatusCode = 400, Description = "Page unable to be found. Please refresh the page and try again."};

            SubscriberDto Subscriber = null;

            if (this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value != null)
                Subscriber = await _api.SubscriberAsync(Guid.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value), false);

            if(!IsNewSubscriberCheckout && Subscriber == null)
                return new BasicResponseDto{ StatusCode = 400, Description = "No subscriber found. Please either login, or create an account above."};

            // Begin assembling transaction dto to pass to API
            

            ServiceOfferingTransactionDto serviceOfferingTransactionDto = new ServiceOfferingTransactionDto();

            SignUpDto signUpDto = null;

            if(IsNewSubscriberCheckout){
                signUpDto = new SignUpDto{
                    email = serviceCheckoutViewModel.NewSubscriberEmail,
                    password = serviceCheckoutViewModel.NewSubscriberPassword,
                    verifyUrl = _configuration["Environment:BaseUrl"].TrimEnd('/') + "/email/confirm-verification/",
                };
            }

            serviceOfferingTransactionDto.SignUpDto = signUpDto;
            


            PromoCodeDto promoCodeDto = null;
            
            if(serviceCheckoutViewModel.PromoCodeEntered != null && !serviceCheckoutViewModel.PromoCodeEntered.Equals(string.Empty)){
                promoCodeDto = await _api.ServiceOfferingPromoCodeValidationAsync(serviceCheckoutViewModel.PromoCodeEntered, serviceCheckoutViewModel.PackageServiceViewModel.PackageId);
            };

            decimal PricePaid = promoCodeDto != null ? promoCodeDto.FinalCost : Decimal.Parse(packagePage.Data.Fields.Price);  

            BraintreePaymentDto braintreePaymentDto = AssembleBraintreePaymentDto(serviceCheckoutViewModel, packagePage, Subscriber, IsNewSubscriberCheckout, PricePaid);
            serviceOfferingTransactionDto.BraintreePaymentDto = braintreePaymentDto;

            SubscriberDto ServiceOfferingSubscriber = new SubscriberDto();

            if(!IsNewSubscriberCheckout){
                ServiceOfferingSubscriber.SubscriberGuid = Subscriber.SubscriberGuid;
            }

            ServiceOfferingOrderDto serviceOfferingOrderDto = new ServiceOfferingOrderDto{
                PromoCode = promoCodeDto,
                ServiceOffering = new ServiceOfferingDto{
                    ServiceOfferingGuid = Guid.Parse(serviceCheckoutViewModel.PackageServiceViewModel.PackageId),
                    Name = packagePage.Data.Fields.PackageName,
                    Price = Decimal.Parse(packagePage.Data.Fields.Price)
                },
                Subscriber = ServiceOfferingSubscriber,
                PricePaid = PricePaid
            };
            serviceOfferingTransactionDto.ServiceOfferingOrderDto = serviceOfferingOrderDto;

            BasicResponseDto Response = await _api.SubmitServiceOfferingPayment(serviceOfferingTransactionDto);

            return Response;
        }

        // This method assumes the two passwords match, and the email is valid, or
        // all fields are blank.
        private bool IsCheckoutSignUp(ServiceCheckoutViewModel serviceCheckoutViewModel){
            string NewSubEmail = serviceCheckoutViewModel.NewSubscriberEmail;
            string NewSubPassword = serviceCheckoutViewModel.NewSubscriberPassword;
            string NewSubPasswordReenter = serviceCheckoutViewModel.NewSubscriberReenterPassword;
            bool AgreeToTerms = serviceCheckoutViewModel.PackageAgreeToTermsAndConditions;

            // Validate all fields have been filled out
            if(NewSubEmail == null || NewSubEmail.Equals(string.Empty) || 
                NewSubPassword == null || NewSubPassword.Equals(string.Empty) ||
                NewSubPasswordReenter == null || NewSubPasswordReenter.Equals(string.Empty) ||
                AgreeToTerms == false)
                return false;

            return true;
        }

        private bool PasswordsMatch(ServiceCheckoutViewModel serviceCheckoutViewModel){
            if(serviceCheckoutViewModel.NewSubscriberPassword != null &&
                serviceCheckoutViewModel.NewSubscriberReenterPassword != null &&
                !serviceCheckoutViewModel.NewSubscriberPassword.Equals(string.Empty) && 
                !serviceCheckoutViewModel.NewSubscriberReenterPassword.Equals(string.Empty) && 
                serviceCheckoutViewModel.NewSubscriberPassword.Equals(serviceCheckoutViewModel.NewSubscriberReenterPassword)){
                return true;
            }
            return false;
        }

        private BraintreePaymentDto AssembleBraintreePaymentDto(ServiceCheckoutViewModel serviceCheckoutViewModel, PageResponse<PackageServiceViewModel> packagePage, SubscriberDto Subscriber, bool IsNewSubscriberCheckout, decimal PricePaid){
            BraintreePaymentDto braintreePaymentDto = new BraintreePaymentDto(){
                PaymentAmount = PricePaid,
                Nonce = serviceCheckoutViewModel.PaymentMethodNonce,
                FirstName = serviceCheckoutViewModel.BillingFirstName,
                LastName = serviceCheckoutViewModel.BillingLastName,
                PhoneNumber = serviceCheckoutViewModel.SubscriberPhoneNumber,
                Email = IsNewSubscriberCheckout ? serviceCheckoutViewModel.NewSubscriberEmail : Subscriber.Email,
                Address = serviceCheckoutViewModel.BillingAddress,
                Locality = serviceCheckoutViewModel.BillingCity,
                StateGuid = serviceCheckoutViewModel.SelectedState,
                CountryGuid = serviceCheckoutViewModel.SelectedCountry,
                MerchantAccountId = braintreeConfiguration.GetConfigurationSetting("Braintree:MerchantAccountID")
            };

            return braintreePaymentDto;
        }
    }
}