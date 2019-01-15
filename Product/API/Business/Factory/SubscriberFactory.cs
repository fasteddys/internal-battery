﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.Business.Resume;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Business.Factory
{
    public class SubscriberFactory
    {
        public static Subscriber GetSubscriberById(UpDiddyDbContext db, int subscriberId)
        {
            return db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
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
            subscriber.PhoneNumber = Utils.RemoveNonNumericCharacters(contactInfo.PhoneNumber);
            subscriber.City = contactInfo.City;
            subscriber.Address = contactInfo.Address;
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
