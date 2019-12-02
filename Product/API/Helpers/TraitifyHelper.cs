using System;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using System.Threading.Tasks;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using static UpDiddyLib.Helpers.Constants;
using UpDiddyApi.ApplicationCore.Services;
namespace UpDiddyApi.Helpers
{
    public static class TraitifyHelper
    {
        public static async Task CompleteSignup(string assessmentId
        , Subscriber subscriber
        , ILogger logger
        , IRepositoryWrapper repositoryWrapper
        , ISysEmail sysEmail
        , IConfiguration config
        , ZeroBounceApi zeroBounceApi)
        {
            try
            {
                UpDiddyApi.Models.Traitify traitify = await repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
                traitify.SubscriberId = subscriber.SubscriberId;
                traitify.ModifyDate = DateTime.UtcNow;
                traitify.Email = subscriber.Email;
                await repositoryWrapper.TraitifyRepository.SaveAsync();
                // Models.Action action = await repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.TraitifyAccountCreation);
                // EntityType entityType = await repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.TraitifyAssessment);
                // await trackingService.TrackSubscriberAction(subscriber.SubscriberId, action, entityType, traitify.Id);
                await SendCompletionEmail(assessmentId, traitify.Email, repositoryWrapper, sysEmail, config, zeroBounceApi);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"TraitifyController.CompleteSignup: An error occured while attempting to complete signup Message: {e.Message}", e);
            }
        }

        public static async Task SendCompletionEmail(string assessmentId
        , string sendTo
        , IRepositoryWrapper repositoryWrapper
        , ISysEmail sysEmail
        , IConfiguration config
        , ZeroBounceApi zeroBounceApi)
        {
            string results = await GetJsonResults(assessmentId, config);
            dynamic payload = await ProcessResult(results, repositoryWrapper, config);
            bool? isEmailValid = zeroBounceApi.ValidateEmail(sendTo);
            if (isEmailValid != null && isEmailValid.Value == true)
            {
                await sysEmail.SendTemplatedEmailAsync(
                                 sendTo,
                                 config["SysEmail:Transactional:TemplateIds:PersonalityAssessment-ResultsSummary"],
                                 payload,
                                 SendGridAccount.Transactional,
                                 null,
                                 null);
            }
        }

        public static async Task<string> GetJsonResults(string assessmentId, IConfiguration config)
        {
            string apiResponse = string.Empty;
            var url = config["Traitify:ResultUrl"].Replace("{assessmentId}", assessmentId);
            using (var httpClient = new HttpClient())
            {
                string publicKey = config["Traitify:PublicKey"];
                var byteArray = Encoding.ASCII.GetBytes($"{publicKey}:x");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                using (var response = await httpClient.GetAsync(url))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return apiResponse;
        }

        private static async Task<dynamic> ProcessResult(string rawData, IRepositoryWrapper repositoryWrapper, IConfiguration config)
        {
            var result = JObject.Parse(rawData);
            dynamic jObj = new JObject();
            jObj.blend = (JObject)result["personality_blend"];
            jObj.types = (JArray)result["personality_types"];
            jObj.courses = await GetSuggestedCourses(jObj.blend, repositoryWrapper, config);
            foreach (var type in jObj.types)
            {
                decimal score = (decimal)type["score"];
                type["score"] = Math.Round(score, 0);
            }
            var traitRef = (JArray)result["personality_traits"];
            var traits = new JArray();
            foreach (var trait in traitRef)
            {
                var d = (decimal)trait["score"];
                decimal roundScore = Math.Round(d, 0);
                if (roundScore > 0)
                {
                    trait["score"] = roundScore;
                    traits.Add(trait);
                }
            }
            jObj.traits = traits;
            return jObj;
        }

        private static async Task<dynamic> GetSuggestedCourses(dynamic blend, IRepositoryWrapper repositoryWrapper, IConfiguration config)
        {
            string type1 = blend["personality_type_1"].SelectToken("name").Value;
            string type2 = blend["personality_type_2"].SelectToken("name").Value;
            var mapping = await repositoryWrapper.TraitifyCourseTopicBlendMappingRepository.GetByPersonalityTypes(type1, type2);
            var courses = new JArray();
            string baseUrl = config["Environment:BaseUrl"];
            baseUrl = baseUrl.Substring(0, (baseUrl.Length - 1));
            if (mapping != null)
            {
                if (!string.IsNullOrEmpty(mapping.TopicOneName))
                {
                    dynamic course1 = new JObject();
                    course1.imgUrl = mapping.TopicOneImgUrl;
                    course1.courseUrl = baseUrl + mapping.TopicOneUrl;
                    courses.Add(course1);
                }

                if (!string.IsNullOrEmpty(mapping.TopicTwoName))
                {
                    dynamic course2 = new JObject();
                    course2.imgUrl = mapping.TopicTwoImgUrl;
                    course2.courseUrl = baseUrl + mapping.TopicTwoUrl;
                    courses.Add(course2);
                }

                if (!string.IsNullOrEmpty(mapping.TopicThreeName))
                {
                    dynamic course3 = new JObject();
                    course3.imgUrl = mapping.TopicThreeImgUrl;
                    course3.courseUrl = baseUrl + mapping.TopicThreeUrl;
                    courses.Add(course3);
                }
            }
            return courses;
        }


    }
}