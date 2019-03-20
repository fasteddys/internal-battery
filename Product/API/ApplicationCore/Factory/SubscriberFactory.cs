using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberFactory
    {

        // Can we get rid of this function given the one below it?
        // JAB - It is being used by the resume parse methods.  Refactor rquired for removal
        public static Subscriber GetSubscriberById(UpDiddyDbContext db, int subscriberId)
        {
            return db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .FirstOrDefault();
        }

        public static SubscriberDto GetSubscriber(UpDiddyDbContext _db, Guid subscriberGuid, ILogger _syslog, IMapper _mapper)
        {
            if (Guid.Empty.Equals(subscriberGuid))
            {
                _syslog.Log(LogLevel.Information, $"***** SubscriberFactory:GetSubscriber empty subscriber guid supplied.");
                return new SubscriberDto();
            }


            Subscriber subscriber = _db.Subscriber
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
                .FirstOrDefault();

            SubscriberDto subscriberDto = _mapper.Map<SubscriberDto>(subscriber);

            if (subscriber != null)
            {
                var eligibleCampaigns =
                    _db.Contact
                    .Where(co => co.IsDeleted == 0 && co.Subscriber.SubscriberId == subscriber.SubscriberId)
                    .Join(
                        _db.CampaignContact.Where(cc => cc.IsDeleted == 0),
                        co => co.ContactId,
                        cc => cc.ContactId,
                        (co, cc) => cc)
                    .Join(
                        _db.Campaign.Where(ca => ca.IsDeleted == 0),
                        cc => cc.CampaignId,
                        ca => ca.CampaignId,
                        (cc, ca) => ca)
                    .Where(ca => ca.StartDate <= DateTime.UtcNow && (!ca.EndDate.HasValue || ca.EndDate.Value >= DateTime.UtcNow))
                    .ToList();
                
                /*
                var eligibleCampaignsBadSql = _db.Campaign
                    .Include(c => c.CampaignCourseVariant).ThenInclude(ccv => ccv.RebateType)
                    .Include(c => c.CampaignCourseVariant).ThenInclude(ccv => ccv.CourseVariant)
                    .Include(c => c.CampaignCourseVariant).ThenInclude(ccv => ccv.Campaign)
                    .Include(c => c.CampaignContact).ThenInclude(cc => cc.Contact).ThenInclude(co => co.Subscriber)
                    .Where(c => c.IsDeleted == 0
                        // the subscriber is associated with a campaign (contact must be linked to subscriber!)
                        && c.CampaignContact.Where(cc => cc.Contact.Subscriber.SubscriberId == subscriber.SubscriberId).Any()
                        // the campaign is active (enrollment dates)
                        && c.StartDate <= DateTime.UtcNow && (!c.EndDate.HasValue || c.EndDate.Value >= DateTime.UtcNow))
                    .ToList();
                */

                subscriberDto.EligibleCampaigns = _mapper.Map<List<CampaignDto>>(eligibleCampaigns);

                // Populate campaign promotional message for enrollment
                // todo find a better way to do  this
                foreach (EnrollmentDto e in subscriberDto.Enrollments)
                    e.CampaignCourseStatusInfo = CampaignFactory.EnrollmentPromoStatusAsText(e);

                subscriberDto.CampaignOffer = CampaignFactory.OpenOffers(_db, subscriberDto.EligibleCampaigns, subscriberDto.Enrollments);

                // todo move to when creating subscriber once we have automated process that will associate the subscriber to the contact upon contact
                // creation.  When that is done, it will only be necessary to do this check when we create  a new subscriber since the create contact logic
                // will handle the case of associating the contact with existing subscribers
                ContactFactory.AssociateSubscriber(_db, subscriberDto.Email, subscriber.SubscriberId);

                subscriberDto.HasVerificationEmail = subscriber.EmailVerification != null;
            }


            return subscriberDto;
        }
        public static Subscriber GetSubscriberByGuid(UpDiddyDbContext db, Guid subscriberGuid)
        {
            return db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();
        }


        public static ProfileDataStatus ImportLinkedIn(UpDiddyDbContext db, ISovrenAPI sovrenApi, SubscriberProfileStagingStore info, ref string msg)
        {
            try
            {
                // Get the subscriber        
                Subscriber subscriber = SubscriberFactory.GetSubscriberById(db, info.SubscriberId);
                if (subscriber == null)
                {
                    msg = $"SubscriberSkill:ImportSovren -> Subscriber {info.SubscriberId} was not found";
                    return ProfileDataStatus.ProcessingError;
                }

                // Convert the linked in data to a resume of sorts
                string base64Resume = _LinkedInResume(info.ProfileData);

                // todo: submit as background job, maybe depends on onboarding flow
                String parsedDocument = sovrenApi.SubmitResumeAsync(base64Resume).Result;

                List<string> skills = Utils.ParseSkillsFromHrXML(parsedDocument);
                _AddSubscriberSkills(db, subscriber, skills);

                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberSkill:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }

        }

        public static ProfileDataStatus ImportSovren(UpDiddyDbContext db, SubscriberProfileStagingStore info, ref string msg, ILogger syslog)
        {
            try
            {
                // Get the subscriber 
                Subscriber subscriber = SubscriberFactory.GetSubscriberById(db, info.SubscriberId);
                if (subscriber == null)
                {
                    msg = $"SubscriberSkill:ImportSovren -> Subscriber {info.SubscriberId} was not found";
                    return ProfileDataStatus.ProcessingError;
                }

                // Import Contact Info 
                _ImportSovrenContactInfo(db, subscriber, info.ProfileData, syslog);
                // Import skills 
                _ImportSovrenSkills(db, subscriber, info.ProfileData, syslog);
                // Import work history  
                _ImportSovrenWorkHistory(db, subscriber, info.ProfileData, syslog);
                // Import education history  
                _ImportSovrenEducationHistory(db, subscriber, info.ProfileData, syslog);

                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberSkill:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }
        }





        #region Helper Functions

        private static bool _ImportSovrenEducationHistory(UpDiddyDbContext db, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                List<SubscriberEducationHistoryDto> eductionHistory = Utils.ParseEducationHistoryFromHrXml(profileData);
                _AddSubscriberEducationHistory(db, subscriber, eductionHistory);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenEducationHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }



        private static bool _ImportSovrenWorkHistory(UpDiddyDbContext db, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                List<SubscriberWorkHistoryDto> workHistory = Utils.ParseWorkHistoryFromHrXml(profileData);
                _AddSubscriberWorkHistory(db, subscriber, workHistory);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenWorkHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }




        private static bool _ImportSovrenContactInfo(UpDiddyDbContext db, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                SubscriberContactInfoDto contactInfo = Utils.ParseContactInfoFromHrXML(profileData);
                _AddSubscriberContactInfo(db, subscriber, contactInfo);
                return true;
            }
            catch (Exception e)
            {
                syslog.Log(LogLevel.Error, $"Subscriber:_ImportSovrenSkills threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {profileData}");
                return false;
            }
        }



        private static bool _ImportSovrenSkills(UpDiddyDbContext db, Subscriber subscriber, string profileData, ILogger syslog)
        {
            try
            {
                List<string> skills = Utils.ParseSkillsFromHrXML(profileData);
                _AddSubscriberSkills(db, subscriber, skills);
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



        private static void _AddSubscriberSkills(UpDiddyDbContext db, Subscriber subscriber, List<string> skills)
        {
            foreach (string skillName in skills)
            {
                Skill skill = SkillFactory.GetOrAdd(db, skillName);
                // Check to see if the subscriber already has that skill 
                SubscriberSkill subscriberSkill = SubscriberSkillFactory.GetSkillForSubscriber(db, subscriber, skill);
                // If the subscriber does not have the skill, add it to their profile 
                if (subscriberSkill == null)
                    SubscriberSkillFactory.AddSkillForSubscriber(db, subscriber, skill);
            }
        }

        private static void _AddSubscriberWorkHistory(UpDiddyDbContext db, Subscriber subscriber, List<SubscriberWorkHistoryDto> workHistoryList)
        {
            foreach (SubscriberWorkHistoryDto wh in workHistoryList)
            {

                Company company = CompanyFactory.GetOrAdd(db, wh.Company);
                SubscriberWorkHistory workHistory = SubscriberWorkHistoryFactory.GetWorkHistoryForSubscriber(db, subscriber, company, wh.StartDate, wh.EndDate);
                if (workHistory == null)
                    SubscriberWorkHistoryFactory.AddWorkHistoryForSubscriber(db, subscriber, wh, company);
            }
        }


        private static void _AddSubscriberContactInfo(UpDiddyDbContext db, Subscriber subscriber, SubscriberContactInfoDto contactInfo)
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
            State state = StateFactory.GetStateByStateCode(db, contactInfo.State);
            if (state != null)
                subscriber.StateId = state.StateId;

            db.SaveChanges();
        }


        private static void _AddSubscriberEducationHistory(UpDiddyDbContext db, Subscriber subscriber, List<SubscriberEducationHistoryDto> educationHistoryList)
        {
            foreach (SubscriberEducationHistoryDto eh in educationHistoryList)
            {
                EducationalInstitution educationalInstitution = EducationalInstitutionFactory.GetOrAdd(db, eh.EducationalInstitution);
                EducationalDegree educationalDegree = EducationalDegreeFactory.GetOrAdd(db, eh.EducationalDegree);
                EducationalDegreeType educationalDegreeType = EducationalDegreeTypeFactory.GetOrAdd(db, eh.EducationalDegreeType);

                SubscriberEducationHistory educationHistory = SubscriberEducationHistoryFactory.GetEducationHistoryForSubscriber(db, subscriber, educationalInstitution, educationalDegree, eh.StartDate, eh.EndDate, eh.DegreeDate);
                if (educationHistory == null)
                    SubscriberEducationHistoryFactory.AddEducationHistoryForSubscriber(db, subscriber, eh, educationalInstitution, educationalDegree, educationalDegreeType);
            }
        }

        #endregion
    }
}
