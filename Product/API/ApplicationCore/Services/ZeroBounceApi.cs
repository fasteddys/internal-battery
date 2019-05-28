using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ZeroBounceApi
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _syslog;

        public ZeroBounceApi(IConfiguration configuration, IRepositoryWrapper repositoryWrapper, ILogger sysLog)
        {
            _apiKey = configuration["ZeroBounce:ApiKey"];
            _apiUrl = configuration["ZeroBounce:ApiUrl"];
            _repositoryWrapper = repositoryWrapper;
            _syslog = sysLog;
        }

        public bool? ValidatePartnerContactEmail(int partnerContactId, string email, int verificationFailureLeadStatusId, string ipAddress = null)
        {
            bool? isValidEmail = null;
            try
            {
                // check to see if we have validated this email already
                bool? priorValidationCheck = _repositoryWrapper.ZeroBounceRepository.MostRecentResultInLast90Days(email).Result;

                // do we have a ZeroBounce validation result within the last 90 days for this email address?
                if (priorValidationCheck.HasValue)
                {
                    // if a prior request exists, set the result to match the prior validation check
                    isValidEmail = priorValidationCheck.Value;
                }
                else                
                {
                    // if there are no prior requests, then we are going to call the service
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    long elapsedTimeInMilliseconds;
                    // call api, get response
                    string response;
                    string httpStatus;
                    using (var client = new HttpClient())
                    {
                        UriBuilder builder = new UriBuilder(_apiUrl);
                        builder.Query += "api_key=" + _apiKey;
                        builder.Query += "&email=" + email;
                        builder.Query += "&ip_address=" + (ipAddress == null ? string.Empty : ipAddress);
                        var request = new HttpRequestMessage()
                        {
                            RequestUri = builder.Uri,
                            Method = HttpMethod.Get
                        };
                        stopwatch.Start();
                        var result = client.SendAsync(request).Result;
                        stopwatch.Stop();
                        elapsedTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
                        response = result.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)result.StatusCode).ToString();
                    }
                    dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);

                    // interpret response
                    isValidEmail = jsonResponse.status == "valid";

                    // write the history of the zero bounce request to our db
                    _repositoryWrapper.ZeroBounceRepository.Create(new ZeroBounce()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ElapsedTimeInMilliseconds = Convert.ToInt32(elapsedTimeInMilliseconds),
                        HttpStatus = httpStatus,
                        IsDeleted = 0,
                        PartnerContactId = partnerContactId,
                        Response = jsonResponse,
                        ZeroBounceGuid = Guid.NewGuid()
                    });

                    // save the changes - it is important to wait until this completes because the second save can cause an exception which prevents
                    // the lead status from being stored (A second operation started on this context before a previous operation completed)
                    _repositoryWrapper.ZeroBounceRepository.SaveAsync().Wait();
                }

                // store lead status if we successfully called the ZeroBounce service and they indicated that the email is invalid
                if (isValidEmail.HasValue && !isValidEmail.Value)
                {
                    _repositoryWrapper.PartnerContactLeadStatusRepository.Create(new PartnerContactLeadStatus()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        LeadStatusId = verificationFailureLeadStatusId,
                        PartnerContactId = partnerContactId,
                        PartnerContactLeadStatusGuid = Guid.NewGuid()
                    });

                    // save the changes - it is important to wait until this completes because if not we will see the following exception:
                    // This SqlTransaction has completed; it is no longer usable.
                    _repositoryWrapper.PartnerContactLeadStatusRepository.SaveAsync().Wait();
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** ZeroBounceApi.ValidatePartnerContactEmail encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }

            return isValidEmail;
        }
    }
}
