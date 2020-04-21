using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Authorization;
using UpDiddyApi.Models;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using UpDiddyApi.Workflow;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;
using static UpDiddyLib.Helpers.Constants;
using G2Interfaces = UpDiddyApi.ApplicationCore.Interfaces.Business.G2;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SendGridService : ISendGridService
    {
        private readonly IConfiguration _configuration;
        private readonly IG2Service _g2Service;
        private readonly IHangfireService _hangfireService;
        private readonly G2Interfaces.IProfileService _profileService;
        private readonly G2Interfaces.IWishlistService _wishlistService;
        private readonly G2Interfaces.ICommentService _commentService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITagService _tagService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _syslog;
        private readonly ISysEmail _sysEmail;

        public SendGridService(IServiceProvider services, IRepositoryWrapper repositoryWrapper, IMapper mapper, IHangfireService hangfireService, IConfiguration configuration, ILogger<SendGridService> logger, ISysEmail sysEmail)
        {

            _configuration = services.GetService<IConfiguration>();
            _g2Service = services.GetService<IG2Service>();
            _hangfireService = services.GetService<IHangfireService>();
            _profileService = services.GetService<G2Interfaces.IProfileService>();
            _wishlistService = services.GetService<G2Interfaces.IWishlistService>();
            _commentService = services.GetService<G2Interfaces.ICommentService>();
            _emailTemplateService = services.GetService<IEmailTemplateService>();
            _tagService = services.GetService<ITagService>();
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _syslog = logger;
            _sysEmail = sysEmail;
        }

   
        public async Task<bool> SendBulkEmailByList(Guid TemplateGuid, List<Guid> Profiles, Guid recruiterSubscriberGuid)
        {
            // validate profiles have been specified 
            if (Profiles == null || Profiles.Count == 0)
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList  One or more profiles must be specified for bulk email");

            // validate the email template 
            EmailTemplate template = await _repositoryWrapper.EmailTemplateRepository.GetByGuid(TemplateGuid);

            if (template == null)
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList {TemplateGuid} is not a valid bulk email template");

            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberGuid(recruiterSubscriberGuid);

            // except out oif recruiter is not found 
            if (recruiter == null)
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList Subscriber {recruiterSubscriberGuid} is not a recruiter");

            // get list of recruiters companies 
            List<RecruiterCompany> recruiterCompanies  = _repositoryWrapper.RecruiterCompanyRepository.GetAll()   
                 .Include(c => c.Company)
                 .Where(rc => rc.IsDeleted == 0 && rc.RecruiterId == recruiter.RecruiterId)
                 .ToList();
      
            // validate the api key 
            if (ValidateSendgridSubAccount(template.SendGridSubAccount) == false)
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList {template.SendGridSubAccount} is not a valid SendGrid subaccount");

            // get the list of email associated with the profiles irregardless of if they are associated with the recruiters company 
            List<UpDiddyApi.Models.G2.Profile> profiles = await _profileService.GetProfilesByGuidList(Profiles);

            foreach (Models.G2.Profile p in profiles)
            {
                dynamic templateData = null;
                try
                {
                    templateData = BuildEmailTemplateData(template.TemplateParams, p, recruiter);
                }
                catch (Exception ex)
                {
                    _syslog.LogError($"SendGridService:SendbulkEmailByList  Error generating email data for {p.ProfileGuid} Error = {ex.Message} ");
                }

                try
                {
                    // remove the profile that have been found from the list of passed profiles.  Do this first to make extra sure its done just 
                    // in some of the following code excepts out.
                    Profiles.Remove(p.ProfileGuid);
                    // map account type to enum 
                    SendGridAccount accountType = (SendGridAccount)Enum.Parse(typeof(SendGridAccount), template.SendGridSubAccount);
                    // send the email if the profile is associated with the recruiters company                    
                   if ( recruiterCompanies.Any(c => c.CompanyId == p.CompanyId)  )
                   {
                        _sysEmail.SendTemplatedEmailAsync(p.Email, template.SendGridTemplateId, templateData, accountType);
                        // add note about email being sent 
                        await AddActivityNote(template.Name, recruiterSubscriberGuid, p.Subscriber.SubscriberId);
                    }
                      
                   else
                      _syslog.LogError($"SendGridService:SendbulkEmailByList  Profile {p.ProfileGuid} is asscociated with any of recruiters {recruiter.RecruiterGuid} companies.  Email not sent for this profile.");
                }
                catch (Exception ex)
                {
                    _syslog.LogError($"SendGridService:SendbulkEmailByList  Error sending email to  {p.Email} Error = {ex.Message} ");
                }
            }

            // Any guids left in the passed list of profiles at this point are Zombies cuz there is code above that removes them as
            // they are processed for bulk emails sends.  We will remove them from the azure index now so they will no longer appear
            // in queries
            _g2Service.G2IndexBulkDeleteByGuidAsync(Profiles);
            
            return true;
        }

        public async Task<bool> SendUserDefinedBulkEmailByList(UserDefinedEmailDto userDefinedEmailDto, Guid recruiterSubscriberGuid)
        {
            // validate profiles have been specified 
            if (userDefinedEmailDto.Profiles == null || userDefinedEmailDto.Profiles.Count == 0)
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList - One or more profiles must be specified for bulk email");

            if (String.IsNullOrWhiteSpace(userDefinedEmailDto.EmailTemplate))
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList - EmailTemplate is missing or invalid");

            if (String.IsNullOrWhiteSpace(userDefinedEmailDto.Subject))
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList - Subject is missing or invalid");

            if (String.IsNullOrWhiteSpace(userDefinedEmailDto.ReplyToEmailAddress))
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList - ReplyToEmailAddress is missing or invalid");

            // validate the email template 
            var sendGridTemplateId = _configuration[$"SysEmail:Transactional:TemplateIds:AdHocEmail"];

            if (String.IsNullOrWhiteSpace(sendGridTemplateId)) 
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList - 'sendGridTemplateId' cannot be null or empty");
            
            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberGuid(recruiterSubscriberGuid);

            // except out oif recruiter is not found 
            if (recruiter == null)
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList Subscriber {recruiterSubscriberGuid} is not a recruiter");

            // get list of recruiters companies 
            List<RecruiterCompany> recruiterCompanies = _repositoryWrapper.RecruiterCompanyRepository.GetAll()
                 .Include(c => c.Company)
                 .Where(rc => rc.IsDeleted == 0 && rc.RecruiterId == recruiter.RecruiterId)
                 .ToList();

            // validate the SendGrid api key 
            var sendGridSubAccount = SendGridAccount.Transactional;
            if (ValidateSendgridSubAccount(sendGridSubAccount.ToString()) == false)
                throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList sendGridSubAccount - {sendGridSubAccount} is not a valid SendGrid subaccount");

            // get the list of email associated with the profiles irregardless of if they are associated with the recruiters company 
            List<UpDiddyApi.Models.G2.Profile> profiles = await _profileService.GetProfilesByGuidList(userDefinedEmailDto.Profiles);

            //Get hydrated emails
            var hydratedEmailTemplates = Services.HydrateEmailTemplateUtility.HydrateEmailTemplates(userDefinedEmailDto.EmailTemplate, recruiter, profiles);

            foreach (HydratedEmailTemplate hydratedEmailTemplate in hydratedEmailTemplates) {
                var profile = profiles.FirstOrDefault(p => hydratedEmailTemplate.Profile != null && p.ProfileGuid == hydratedEmailTemplate.Profile.ProfileGuid);

                if (profile == null) throw new FailedValidationException($"SendGridService:SendUserDefinedBulkEmailByList - 'hydratedEmailTemplates' list contains unwanted profile. 'hydratedEmailTemplate.Profile.ProfileGuid' is not as requested.");

                dynamic templateData = null;
                try
                {
                    // remove the profile that have been found from the list of passed profiles.  Do this first to make extra sure its done just 
                    // in some of the following code excepts out.
                    userDefinedEmailDto.Profiles.Remove(profile.ProfileGuid);

                    // send the email if the profile is associated with the recruiters company                    
                    if (recruiterCompanies.Any(c => c.CompanyId == profile.CompanyId))
                    {
                        templateData = new
                        {
                            content = hydratedEmailTemplate.Value,
                            subject = userDefinedEmailDto.Subject
                        };
                        _sysEmail.SendTemplatedEmailWithReplyToAsync(profile.Email, sendGridTemplateId, templateData, sendGridSubAccount, replyToEmail: userDefinedEmailDto.ReplyToEmailAddress);
                        // add note about email being sent 
                        await AddActivityNote("AdHocEmail", recruiterSubscriberGuid, profile.Subscriber.SubscriberId, userDefinedEmailDto.ActivityNote);
                    }

                    else
                        _syslog.LogError($"SendGridService:SendUserDefinedBulkEmailByList - Profile {profile.ProfileGuid} is asscociated with any of recruiters {recruiter.RecruiterGuid} companies.  Email not sent for this profile.");
                }
                catch (Exception ex)
                {
                    _syslog.LogError($"SendGridService:SendUserDefinedBulkEmailByList - Error sending email to  {profile.Email} Error = {ex.Message} ");
                }
            }

            // Any guids left in the passed list of profiles at this point are Zombies cuz there is code above that removes them as
            // they are processed for bulk emails sends.  We will remove them from the azure index now so they will no longer appear
            // in queries
            _g2Service.G2IndexBulkDeleteByGuidAsync(userDefinedEmailDto.Profiles);

            return true;
        }

        public async Task<EmailTemplateListDto> GetEmailTemplates(int limit, int offset, string sort, string order)
        {
            // get the list of available email templates 
            EmailTemplateListDto rval = await _emailTemplateService.GetEmailTemplates(limit, offset, sort, order);

            return rval;
        }

        private async Task AddActivityNote(string templateName, Guid recruiterSubscriberGuid, int subscriberId, string activityNote = null)
        {
            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberGuid(recruiterSubscriberGuid);
            if (recruiter?.SubscriberId == null) { return; }

            await _repositoryWrapper.SubscriberNotesRepository.AddNotes(new SubscriberNotes
            {
                SubscriberNotesGuid = Guid.NewGuid(),
                SubscriberId = subscriberId,
                RecruiterId = recruiter.RecruiterId,
                IsDeleted = 0,
                ViewableByOthersInRecruiterCompany = true,
                Notes = activityNote ?? $"Template {templateName} bulk email sent by {recruiter.LastName}, {recruiter.FirstName} on {DateTime.Now:G}.",
            });
        }

        #region Private Helpers

        private dynamic BuildEmailTemplateData(string templateParams, Models.G2.Profile profile, Recruiter recruiter)
        {
            var props = templateParams.Split(';')
                .SkipWhile(string.IsNullOrEmpty)
                .Select(s => s.Split(':'))
                .Where(s => s.Length == 2)
                .ToDictionary(s => s[1], s => s[0]);

            JObject rval = new JObject();

            foreach (var keyValuePair in props)
            {
                var objectToTest =
                    keyValuePair.Value.StartsWith("recruiter.", StringComparison.CurrentCultureIgnoreCase) ? recruiter
                    : keyValuePair.Value.StartsWith("profile.", StringComparison.CurrentCultureIgnoreCase) ? profile
                    : null as object;

                if (objectToTest == null)
                {
                    _syslog.LogError($"SendGridService:GetPropertyValue: {keyValuePair.Value} is NOT a valid profile navigation property, skipping...");
                    continue;
                }

                var val = GetPropertyValue(objectToTest, keyValuePair.Value.Remove(0, keyValuePair.Value.IndexOf('.') + 1));
                if (val != null)
                {
                    JToken paramVal = JToken.FromObject(val);
                    rval.Add(keyValuePair.Key, paramVal);
                }
            }
            return rval;
        }


        private object GetPropertyValue(object src, string propName)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);

                var prop = src.GetType().GetProperty(temp[0]);
                if (prop == null)
                {
                    _syslog.LogError($"SendGridService:GetPropertyValue: {temp[0]} is NOT a valid profile navigation property, returning null");
                    return null;
                }

                var subObject = prop.GetValue(src, null);
                if (subObject == null)
                    return null;

                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                if (prop == null)
                    _syslog.LogError($"SendGridService:GetPropertyValue: {propName} is NOT a valid profile property, returning null");
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }



        private bool ValidateSendgridSubAccount(string accountName)
        {
            string apiKey = _configuration[$"SysEmail:{accountName}:ApiKey"];
            return apiKey != null;
        }

        #endregion


    }
}
