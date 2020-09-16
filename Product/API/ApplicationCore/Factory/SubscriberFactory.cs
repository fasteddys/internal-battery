using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberFactory
    {
        // Can we get rid of this function given the one below it?
        // JAB - It is being used by the resume parse methods.  Refactor rquired for removal
        public static async Task<Subscriber> GetSubscriberById(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.SubscriberRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .FirstOrDefaultAsync();
        }

        public static string JobseekerUrl(IConfiguration config, Guid subscriberGuid)
        {

            return config["CareerCircle:ViewTalentUrl"] + subscriberGuid;
        }
        
        /// <summary>
        /// Updates the subscriber to indicate the last time they signed in. We could add a computed column 
        /// to infer whether or not they are verified based on this data, since users may not sign in without
        /// verifying their email address via Auth0.
        /// </summary>
        /// <param name="_db"></param>
        /// <param name="subscriberGuid"></param>
        public static void UpdateLastSignIn(UpDiddyDbContext _db, Guid subscriberGuid)
        {
            Subscriber subscriber = _db.Subscriber
                .Where(s => s.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();

            if(subscriber != null)
            {
                var currentTime = DateTime.UtcNow;
                subscriber.ModifyDate = currentTime;
                subscriber.LastSignIn = currentTime;
                subscriber.ModifyGuid = Guid.Empty;
                _db.SaveChanges();
            }
        }

        public static async Task<SubscriberDto> GetSubscriber(IRepositoryWrapper repositoryWrapper, Guid subscriberGuid, ILogger _syslog, IMapper _mapper)
        {
            if (Guid.Empty.Equals(subscriberGuid))
            {
                _syslog.Log(LogLevel.Information, $"***** SubscriberFactory:GetSubscriber empty subscriber guid supplied.");
                return new SubscriberDto();
            }


            Subscriber subscriber = await repositoryWrapper.SubscriberRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .Include(s => s.EmailVerification)
                .Include(s => s.State).ThenInclude(c => c.Country)
                .Include(s => s.SubscriberSkills).ThenInclude(ss => ss.Skill)
                .Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .Include(s => s.Enrollments).ThenInclude(e => e.CampaignCourseVariant).ThenInclude(r => r.CourseVariant)
                .Include(s => s.Enrollments).ThenInclude(e => e.CampaignCourseVariant).ThenInclude(r => r.RebateType)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(swh => swh.Company)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(swh => swh.CompensationType)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalInstitution)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalDegreeType)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalDegree)
                .Include(s => s.SubscriberFile)
                .Include(s => s.SubscriberNotifications).ThenInclude(sn => sn.Notification)
                .FirstOrDefaultAsync();

            SubscriberDto subscriberDto = _mapper.Map<SubscriberDto>(subscriber);

            if (subscriber != null)
            {
                var eligibleCampaigns =
                   await repositoryWrapper.PartnerContactRepository.GetAllWithTracking()
                    .Where(pc => pc.IsDeleted == 0 && pc.Contact.Subscriber.SubscriberId == subscriber.SubscriberId)
                    .Join(
                        repositoryWrapper.CampaignPartnerContactRepository.GetAllWithTracking().Where(cpc => cpc.IsDeleted == 0),
                        pc => pc.PartnerContactId,
                        cpc => cpc.PartnerContactId,
                        (pc, cpc) => cpc)
                    .Join(
                        repositoryWrapper.CampaignRepository.GetAllWithTracking().Where(ca => ca.IsDeleted == 0),
                        cpc => cpc.CampaignId,
                        ca => ca.CampaignId,
                        (cpc, ca) => ca)
                    .Include(c => c.CampaignCourseVariant).ThenInclude(ccv => ccv.CourseVariant)
                    .Include(c => c.CampaignCourseVariant).ThenInclude(ccv => ccv.RebateType)
                    .Where(ca => ca.StartDate <= DateTime.UtcNow && (!ca.EndDate.HasValue || ca.EndDate.Value >= DateTime.UtcNow))
                    .ToListAsync();

                subscriberDto.EligibleCampaigns = _mapper.Map<List<CampaignDto>>(eligibleCampaigns);

                // Populate campaign promotional message for enrollment
                // todo find a better way to do  this
                foreach (EnrollmentDto e in subscriberDto.Enrollments)
                    e.CampaignCourseStatusInfo = CampaignFactory.EnrollmentPromoStatusAsText(e);

                subscriberDto.CampaignOffer = await CampaignFactory.OpenOffers(repositoryWrapper, subscriberDto.EligibleCampaigns, subscriberDto.Enrollments);

                // todo move to when creating subscriber once we have automated process that will associate the subscriber to the contact upon contact
                // creation.  When that is done, it will only be necessary to do this check when we create  a new subscriber since the create contact logic
                // will handle the case of associating the contact with existing subscribers
                await ContactFactory.AssociateSubscriber(repositoryWrapper, subscriberDto.Email, subscriber.SubscriberId);

                subscriberDto.HasVerificationEmail = subscriber.EmailVerification != null;
            }


            return subscriberDto;
        }
        public static async Task<Subscriber> GetSubscriberByGuid(IRepositoryWrapper repositoryWrapper, Guid subscriberGuid)
        {
            return await repositoryWrapper.SubscriberRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .FirstOrDefaultAsync();
        }

        public static async Task<Subscriber> GetSubscriberProfileByGuid(IRepositoryWrapper repositoryWrapper, Guid subscriberGuid)
        {
            Subscriber subscriber = await repositoryWrapper.SubscriberRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(e => e.Company)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(i => i.EducationalInstitution)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(i => i.EducationalDegree)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(i => i.EducationalDegreeType)
                .Include(s => s.State)
                .FirstOrDefaultAsync();


            return subscriber;
        }

        public static async Task<Subscriber> GetDeletedSubscriberProfileByGuid(IRepositoryWrapper repositoryWrapper, Guid subscriberGuid)
        {
            Subscriber subscriber = await repositoryWrapper.SubscriberRepository.GetAllWithTracking()
                .Where(s => s.SubscriberGuid == subscriberGuid)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(e => e.Company)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(i => i.EducationalInstitution)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(i => i.EducationalDegree)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(i => i.EducationalDegreeType)
                .Include(s => s.State)
                .FirstOrDefaultAsync();
            return subscriber;
        }

        public static async Task<Subscriber> GetSubscriberWithSubscriberFiles(IRepositoryWrapper repositoryWrapper, Guid subscriberGuid)
        {
            return await repositoryWrapper.SubscriberRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .Include(s => s.SubscriberFile)
                .FirstOrDefaultAsync();
        }


        public static bool RemoveAvatar(IRepositoryWrapper repositoryWrapper, IConfiguration config, Guid subscriberGuid, ref string msg)
        {

            int MaxAvatarFileSize = int.Parse(config["CareerCircle:MaxAvatarFileSize"]);
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(repositoryWrapper, subscriberGuid).Result;
            if (subscriber == null)
            {
                msg = $"SubscriberFactory.ImportLinkedInImage: {subscriberGuid} is not a valid subscriber";
                return false;
            }
            subscriber.AvatarUrl = string.Empty;
            subscriber.ModifyDate = DateTime.Now;
            repositoryWrapper.SubscriberRepository.SaveAsync();

            return true;

        }



        public static bool UpdateAvatar(IRepositoryWrapper repositoryWrapper, IConfiguration config, IFormFile avatarFile, Guid subscriberGuid, ref string msg)
        {

            int MaxAvatarFileSize = int.Parse(config["CareerCircle:MaxAvatarFileSize"]);
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(repositoryWrapper, subscriberGuid).Result;
            if (subscriber == null)
            {
                msg = $"SubscriberFactory.ImportLinkedInImage: {subscriberGuid} is not a valid subscriber";
                return false;
            }

            byte[] imageBytes = new byte[MaxAvatarFileSize];
            using (var ms = new MemoryStream())
            {
                avatarFile.CopyTo(ms);
                imageBytes = ms.ToArray();
            }
            // this won't work but that's okay because this method is no longer used (marking as obsolete)
            AzureBlobStorage abs = new AzureBlobStorage(config,null);
            string blobFilePath = subscriberGuid + config["CareerCircle:AvatarName"];
            abs.UploadBlobAsync(blobFilePath, imageBytes).Wait();
            subscriber.AvatarUrl = blobFilePath;
            subscriber.ModifyDate = DateTime.Now;
            repositoryWrapper.SubscriberRepository.SaveAsync();

            return true;
        }


        [Obsolete()]
        public static bool ImportLinkedInAvatar(IRepositoryWrapper repositoryWrapper, IConfiguration config, string responseJSon, Guid subscriberGuid, ref string msg)
        {
            JObject o = JObject.Parse(responseJSon);
            JToken imageUrlToken = o.SelectToken("$.profilePicture.displayImage~.elements[0].identifiers[0].identifier");
            string imageUrl = imageUrlToken.ToString();
            int MaxAvatarFileSize = int.Parse(config["CareerCircle:MaxAvatarFileSize"]);

            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(repositoryWrapper, subscriberGuid).Result;
            if (subscriber == null)
            {
                msg = $"SubscriberFactory.ImportLinkedInImage: {subscriberGuid} is not a valid subscriber";
                return false;
            }

            byte[] imageBytes = new byte[MaxAvatarFileSize];
            if (Utils.GetImageAsBlob(imageUrl, MaxAvatarFileSize, ref imageBytes))
            {
                // this won't work but that's okay because this method is no longer used (marking as obsolete)
                AzureBlobStorage abs = new AzureBlobStorage(config, null);
                string blobFilePath = subscriberGuid + config["CareerCircle:LinkedInAvatarName"];
                abs.UploadBlobAsync(blobFilePath, imageBytes).Wait();
                subscriber.LinkedInAvatarUrl = blobFilePath;
                if (string.IsNullOrEmpty(subscriber.AvatarUrl))
                    subscriber.AvatarUrl = blobFilePath;
                subscriber.ModifyDate = DateTime.Now;
            }
            else
                msg = $"SubscriberFactory.ImportLinkedInImage: subsscriber {subscriberGuid} has an avatar that is too large";

            subscriber.LinkedInSyncDate = DateTime.Now;
            repositoryWrapper.SubscriberRepository.SaveAsync();

            return true;
        }


        public static ProfileDataStatus ImportLinkedIn(IRepositoryWrapper repositoryWrapper, ISovrenAPI sovrenApi, IMemoryCacheService memoryCacheService, SubscriberProfileStagingStore info, ref string msg)
        {
            try
            {
                // Get the subscriber        
                Subscriber subscriber = SubscriberFactory.GetSubscriberById(repositoryWrapper, info.SubscriberId).Result;
                if (subscriber == null)
                {
                    msg = $"SubscriberSkill:ImportSovren -> Subscriber {info.SubscriberId} was not found";
                    return ProfileDataStatus.ProcessingError;
                }

                // Convert the linked in data to a resume of sorts
                string base64Resume = _LinkedInResume(info.ProfileData);

                // todo: submit as background job, maybe depends on onboarding flow
                String parsedDocument = sovrenApi.SubmitResumeAsync(subscriber.SubscriberId, base64Resume).Result;

                List<string> skills = Utils.ParseSkillsFromHrXML(parsedDocument);
                _AddSubscriberSkills(repositoryWrapper, memoryCacheService,subscriber, skills).Wait();

                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberSkill:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }

        }


        public static ProfileDataStatus ImportSovren(IRepositoryWrapper repositoryWrapper, IMemoryCacheService memoryCache, SubscriberProfileStagingStore info, ref string msg, ILogger syslog)
        {
            try
            {
                // Get the subscriber 
                Subscriber subscriber = SubscriberFactory.GetSubscriberById(repositoryWrapper, info.SubscriberId).Result;
                if (subscriber == null)
                {
                    msg = $"SubscriberFactory:ImportSovren -> Subscriber {info.SubscriberId} was not found";
                    return ProfileDataStatus.ProcessingError;
                }

                // Import Contact Info 
                _ImportSovrenContactInfo(repositoryWrapper, subscriber, info.ProfileData, syslog).Wait();
                // Import skills 
                _ImportSovrenSkills(repositoryWrapper, memoryCache, subscriber, info.ProfileData, syslog).Wait();
                // Import work history  
                _ImportSovrenWorkHistory(repositoryWrapper, subscriber, info.ProfileData, syslog).Wait();
                // Import education history  
                _ImportSovrenEducationHistory(repositoryWrapper, subscriber, info.ProfileData, syslog).Wait();

                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberFactory:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }
        }

        public static async Task<bool> IsEligibleForOffers(IRepositoryWrapper repositoryWrapper, Guid SubscriberGuid, ILogger _syslog, IMapper _mapper, IIdentity Identity)
        {
            SubscriberDto subscriber = await GetSubscriber(repositoryWrapper, SubscriberGuid, _syslog, _mapper);
            if (subscriber == null)
                return false;
            return Identity.IsAuthenticated && subscriber.Files.Count > 0;
        }

        #region Skills

        public static async Task<IList<SubscriberSkill>> GetSubscriberSkillsById(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.SubscriberSkillRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .Include(s => s.Skill)
                .ToListAsync();
        }

        #endregion

        #region Work History

        public static async Task<IList<SubscriberWorkHistory>> GetSubscriberWorkHistoryById(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.SubscriberWorkHistoryRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .Include(s => s.Company)
                .ToListAsync();
        }

        #endregion


        #region Education History

        public static async Task<IList<SubscriberEducationHistory>> GetSubscriberEducationHistoryById(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.SubscriberEducationHistoryRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .Include(s => s.EducationalInstitution)
                .Include(s => s.EducationalDegree)
                .Include(s => s.EducationalDegreeType)
                .ToListAsync();
        }

        #endregion



        #region Helper Functions

        private static async Task<bool> _ImportSovrenEducationHistory(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                List<SubscriberEducationHistoryDto> eductionHistory = Utils.ParseEducationHistoryFromHrXml(profileData);
                await _AddSubscriberEducationHistory(repositoryWrapper, subscriber, eductionHistory);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenEducationHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }



        private static async Task<bool> _ImportSovrenWorkHistory(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                List<SubscriberWorkHistoryDto> workHistory = Utils.ParseWorkHistoryFromHrXml(profileData);
                await _AddSubscriberWorkHistory(repositoryWrapper, subscriber, workHistory);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenWorkHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }




        private static async Task<bool> _ImportSovrenContactInfo(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                SubscriberContactInfoDto contactInfo = Utils.ParseContactInfoFromHrXML(profileData);
                await _AddSubscriberContactInfo(repositoryWrapper, subscriber, contactInfo);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenSkills threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }



        private static async Task<bool> _ImportSovrenSkills(IRepositoryWrapper repositoryWrapper, IMemoryCacheService memoryCache, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                List<string> skills = Utils.ParseSkillsFromHrXML(profileData);
                await _AddSubscriberSkills(repositoryWrapper, memoryCache, subscriber, skills);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenSkills threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }


        private static string _LinkedInResume(string linkedInProfileData)
        {
            StringBuilder tempResume = new StringBuilder();

            JObject o = JObject.Parse(linkedInProfileData);
            JToken firstName = o.SelectToken("$.firstName");
            JToken lastName = o.SelectToken("$.lastName");
            JToken summary = o.SelectToken("$.summary");

            tempResume.Append("Resume for " + firstName.ToString() + " " + lastName.ToString() + Environment.NewLine);
            tempResume.Append("Career Summary: " + summary.ToString() + Environment.NewLine);
            tempResume.Append("Work History" + Environment.NewLine);
            IEnumerable<JToken> positions = o.SelectTokens("$.positions.values[*].summary");
            foreach (JToken workSummary in positions)
                tempResume.Append("Position Summary  " + workSummary.ToString() + Environment.NewLine);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(tempResume.ToString());
            return System.Convert.ToBase64String(plainTextBytes);

        }



        private static async Task _AddSubscriberSkills(IRepositoryWrapper repositoryWrapper, IMemoryCacheService memoryCacheService, Subscriber subscriber, List<string> skills)
        {
            foreach (string skillName in skills)
            {
                Skill skill = await SkillFactory.GetOrAdd(repositoryWrapper, memoryCacheService, skillName);
                // Check to see if the subscriber already has that skill 
                SubscriberSkill subscriberSkill = await SubscriberSkillFactory.GetSkillForSubscriber(repositoryWrapper, subscriber, skill);
                // If the subscriber does not have the skill, add it to their profile 
                if (subscriberSkill == null)
                    await SubscriberSkillFactory.AddSkillForSubscriber(repositoryWrapper, subscriber, skill);
            }
        }

        private static async Task _AddSubscriberWorkHistory(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, List<SubscriberWorkHistoryDto> workHistoryList)
        {
            foreach (SubscriberWorkHistoryDto wh in workHistoryList)
            {

                Company company = CompanyFactory.GetOrAdd(repositoryWrapper, wh.Company).Result;
                SubscriberWorkHistory workHistory = await SubscriberWorkHistoryFactory.GetWorkHistoryForSubscriber(repositoryWrapper, subscriber, company, wh.StartDate, wh.EndDate);
                if (workHistory == null)
                    await SubscriberWorkHistoryFactory.AddWorkHistoryForSubscriber(repositoryWrapper, subscriber, wh, company);
            }
        }


        private static async Task _AddSubscriberContactInfo(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, SubscriberContactInfoDto contactInfo)
        {
            subscriber.FirstName = contactInfo.FirstName;
            subscriber.LastName = contactInfo.LastName;
            contactInfo.PhoneNumber = contactInfo.PhoneNumber.Trim().Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
            Regex phoneRegex = new Regex(@"^([0-9]{0,3})?[2-9]{1}[0-9]{9}$");
            if (phoneRegex.IsMatch(contactInfo.PhoneNumber))
                subscriber.PhoneNumber = Utils.RemoveNonNumericCharacters(contactInfo.PhoneNumber);
            subscriber.City = contactInfo.City;
            subscriber.Address = contactInfo.Address;
            subscriber.PostalCode = contactInfo.PostalCode;
            State state = await StateFactory.GetStateByStateCode(repositoryWrapper, contactInfo.State);
            if (state != null)
                subscriber.StateId = state.StateId;

            await repositoryWrapper.State.SaveAsync();
        }


        private static async Task _AddSubscriberEducationHistory(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, List<SubscriberEducationHistoryDto> educationHistoryList)
        {
            foreach (SubscriberEducationHistoryDto eh in educationHistoryList)
            {
                EducationalInstitution educationalInstitution = await EducationalInstitutionFactory.GetOrAdd(repositoryWrapper, eh.EducationalInstitution);
                EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(repositoryWrapper, eh.EducationalDegree);
                EducationalDegreeType educationalDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(repositoryWrapper, eh.EducationalDegreeType);
                SubscriberEducationHistory educationHistory = await SubscriberEducationHistoryFactory.GetEducationHistoryForSubscriber(repositoryWrapper, subscriber, educationalInstitution, educationalDegree, eh.StartDate, eh.EndDate, eh.DegreeDate);
                if (educationHistory == null)
                    await SubscriberEducationHistoryFactory.AddEducationHistoryForSubscriber(repositoryWrapper, subscriber, eh, educationalInstitution, educationalDegree, educationalDegreeType);
            }

        }

        #endregion
    }
}
