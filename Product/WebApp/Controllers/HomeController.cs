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
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    public class HomeController : Controller
    {
        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IConfiguration _configuration;
   
        public HomeController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<HomeController> localizer, IConfiguration configuration)
        {
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var xxx = _configuration["Api:ApiUrl"];
            // TODO remove test code 
            ApiUpdiddy API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, _configuration);
            var x = API.GetAsString("Values", true);
            var xx = API.Get<string>("Values", true);

            HomeViewModel HomeViewModel = new HomeViewModel(_configuration, API.Topics());
            return View(HomeViewModel);
        }

        public IActionResult Terms()
        {


            return View();
        }

        public IActionResult News()
        {

            return View();
        }


        public IActionResult AboutUs()
        {
 
            return View();
        }

 
        public IActionResult Privacy()
        {
 
            return View();
        }


        public IActionResult WhatWeAreAbout()
        {

            return View();
        }

        public IActionResult About()
        {
    
            return View();
        }

        public IActionResult ContactUs()
        {

            return View();
        }

        [AllowAnonymous]
        public IActionResult Unified()
        {
            return View();
        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Api()
        {
            string responseString = "";
            try
            {

          

                // Retrieve the token with the specified scopes
                var scope = AzureAdB2COptions.ApiScopes.Split(' ');
                string signedInUserID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();
                ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureAdB2COptions.ClientId, AzureAdB2COptions.Authority, AzureAdB2COptions.RedirectUri, new ClientCredential(AzureAdB2COptions.ClientSecret), userTokenCache, null);
                // TODO remove debug var
                var x = cca.Users.FirstOrDefault();
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureAdB2COptions.Authority, false);

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AzureAdB2COptions.ApiUrl);

                // Add token to the Authorization header and make the request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            ViewData["Payload"] = $"{responseString}";            
            return View();
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }


        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl);
        }
    }
}
