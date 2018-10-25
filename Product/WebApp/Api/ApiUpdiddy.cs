﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UpDiddy.Helpers;
using UpDiddyLib.Dto;
using System.Net;


namespace UpDiddy.Api
{
    public class ApiUpdiddy : ApiHelperMsal
    {
    
        public ApiUpdiddy(AzureAdB2COptions Options, HttpContext Context, IConfiguration conifguration) {

            AzureOptions = Options;
            HttpContext = Context;
            _configuration = conifguration;
            // Set the base URI for API calls 
            _ApiBaseUri = _configuration["Api:ApiUrl"];
        }

        public string Hello()
        {
            return Get<string>("hello", true);
        }

        // TODO add application caching  
        public IList<TopicDto> Topics()
        {
            return Get<IList<TopicDto>>("topic", false);
        }

        public TopicDto TopicById(int TopicId)
        {
            return Get<TopicDto>($"topic/{TopicId}", false);
        }

        public TopicDto TopicBySlug(string TopicSlug)
        {
            return Get<TopicDto>("topic/slug/" + TopicSlug, false);
        }

        public IList<CourseDto> getCousesByTopicSlug(string TopicSlug)
        {
            return Get<IList<CourseDto>>("course/" + TopicSlug, false);
        }

        public CourseDto Course(string CourseSlug)
        {
            CourseDto retVal = Get<CourseDto>("course/slug/" + CourseSlug, false);
            return retVal;
        }

        public CourseDto CourseByGuid(Guid CourseGuid)
        {
            CourseDto retVal = Get<CourseDto>("course/guid/" + CourseGuid, false);
            return retVal;
        }

        public SubscriberDto Subscriber(Guid SubscriberGuid)
        {
            return Get<SubscriberDto>("subscriber/" + SubscriberGuid);
        }

        public WozTermsOfServiceDto GetWozTermsOfService()
        {
            return Get<WozTermsOfServiceDto>("woz/TermsOfService/", false);
        }
        public Guid EnrollStudentAndObtainEnrollmentGUID(EnrollmentDto enrollmentDto)
        {
            return Post<Guid>(enrollmentDto, "enrollment/", false);
        }

        public SubscriberDto CreateSubscriber(string SubscriberGuid, string SubscriberEmail)
        {
            return Get<SubscriberDto>("subscriber/createsubscriber/" + SubscriberGuid + "/" + Uri.EscapeDataString(SubscriberEmail),true);
        }

        public PromoCodeDto PromoCodeValidation(string code, string courseGuid, string subscriberGuid)
        {
            return Get<PromoCodeDto>("promocode/" + code + "/" + courseGuid + "/" + subscriberGuid, true);
        }

        public BasicResponseDto UpdateProfileInformation(SubscriberDto Subscriber)
        {
            return Post<BasicResponseDto>(Subscriber, "profile/update", false);
        }

        public IList<CountryStateDto> GetCountryStateList()
        {
            return Get<IList<CountryStateDto>>("profile/LocationList", false);
        }

    }
}


