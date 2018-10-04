﻿using System;
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
        [HttpGet]
        [Route("/Course/Checkout/{CourseSlug}")]
        public IActionResult Get(string CourseSlug)
        {
            GetSubscriber();

            ApiUpdiddy API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, _configuration);
            CourseDto Course = API.Course(CourseSlug);
            TopicDto ParentTopic = API.TopicById(Course.TopicId);
            WozTermsOfServiceDto WozTOS = API.GetWozTermsOfService();
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, Course, this.subscriber, ParentTopic, WozTOS);

            
            return View("Checkout", CourseViewModel);
        }

        [HttpPost]
        public IActionResult Checkout(int TermsOfServiceDocId, string CourseSlug)
        {
            GetSubscriber();
            DateTime dateTime = new DateTime();
            CourseDto Course = API.Course(CourseSlug);
            //ApiUpdiddy API = new ApiUpdiddy(AzureAdB2COptions, this.HttpContext, _configuration);
            EnrollmentDto enrollmentDto = new EnrollmentDto
            {
                CourseId = Course.CourseId,
                EnrollmentGuid = Guid.NewGuid(),
                SubscriberId = this.subscriber.SubscriberId,
                DateEnrolled = dateTime,
                PricePaid = (decimal)Course.Price,
                PercentComplete = 0,
                IsRetake = 0, //TODO Make this check DB for existing entry
                EnrollmentStatusId = 0,
                TermsOfServiceFlag = TermsOfServiceDocId
            };
            API.EnrollStudentAndObtainEnrollmentGUID(enrollmentDto);
            TopicDto ParentTopic = API.TopicById(Course.TopicId);
            WozTermsOfServiceDto WozTOS = API.GetWozTermsOfService();
            CourseViewModel CourseViewModel = new CourseViewModel(_configuration, Course, this.subscriber, ParentTopic, WozTOS);
            return View("EnrollmentSuccess", CourseViewModel);
        }

        public IActionResult EnrollmentSuccess()
        {
            return View();
        }



    }
}
